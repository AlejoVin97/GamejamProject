using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZoneManager : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform player;
    [SerializeField] GameObject[] zones;       // 0 = primera zona (visible al inicio)
    [SerializeField] Tilemap[] zoneGrounds;    // el Ground de cada zona, en el mismo orden
    [SerializeField] float padding = 1f;
    [SerializeField] float zoomTime = 0.8f;    // suavizado del zoom
    [SerializeField] float followTime = 0.2f;  // suavizado al seguir al jugador
    [SerializeField] int edgeDistance = 3;     // casillas al borde de la siguiente zona para alejar
    [SerializeField] float revealHold = 2f;    // segundos que se muestra la zona recién desbloqueada (0 = desactivado)

    [SerializeField] Camera[] fixedCameras;   // one per zone, same order as Zones. Leave None to use the moving camera
    [SerializeField] float arriveTolerance = 0.05f;   // how close the moving camera must be to a fixed view before swapping

    // The camera that is rendering right now (ClickInteractor needs it)
    public Camera ActiveCamera { get; private set; }

    Bounds[] zoneBounds;
    List<Vector3Int>[] zoneCells;
    Vector3 initialPos;
    float initialSize;

    int unlocked = 0;
    int currentZone = 0;
    bool nearNext = false;
    float revealTimer = 0f;
    Vector3Int lastCell = new Vector3Int(int.MinValue, 0, 0);
    Vector3 posVel;
    float sizeVel;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        initialPos = cam.transform.position;
        initialSize = cam.orthographicSize;

        zoneBounds = new Bounds[zones.Length];
        zoneCells = new List<Vector3Int>[zones.Length];
        zoneBounds[0] = CalcBounds(zones[0]);
        for (int i = 1; i < zones.Length; i++) zones[i].SetActive(false);
    }

    // Llamar desde el UnityEvent onInteract del último puzzle de cada zona
    public void UnlockNextZone()
    {
        if (unlocked >= zones.Length - 1) return;

        unlocked++;
        zones[unlocked].SetActive(true);
        zoneBounds[unlocked] = CalcBounds(zones[unlocked]);
        zoneCells[unlocked] = CollectCells(zoneGrounds[unlocked]);

        revealTimer = revealHold;
        lastCell = new Vector3Int(int.MinValue, 0, 0);   // fuerza recalcular estado
    }

    void LateUpdate()
    {
        if (revealTimer > 0f) revealTimer -= Time.deltaTime;

        UpdateZoneState();
        UpdateActiveCamera();

        // A fixed camera is active: nothing to move
        if (ActiveCamera != cam) return;

        GetTarget(out Vector3 pos, out float size, out float posTime);

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, size, ref sizeVel, zoomTime);
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, pos, ref posVel, posTime);
    }

    // Detecta en qué zona está el jugador y si está cerca del borde de la siguiente
    void UpdateZoneState()
    {
        Vector3Int cell = zoneGrounds[0].WorldToCell(player.position);
        if (cell == lastCell) return;
        lastCell = cell;

        for (int i = unlocked; i >= 0; i--)
        {
            if (zoneGrounds[i].HasTile(cell)) { currentZone = i; break; }
        }

        nearNext = false;
        int next = currentZone + 1;
        if (next > unlocked) return;

        foreach (Vector3Int c in zoneCells[next])
        {
            if (Mathf.Abs(c.x - cell.x) + Mathf.Abs(c.y - cell.y) <= edgeDistance)
            {
                nearNext = true;
                break;
            }
        }
    }

    void GetTarget(out Vector3 pos, out float size, out float posTime)
    {
        posTime = zoomTime;

        // 1) Revelar la zona recién desbloqueada
        if (revealTimer > 0f)
        {
            Bounds all = zoneBounds[0];
            for (int i = 1; i <= unlocked; i++) all.Encapsulate(zoneBounds[i]);
            Fit(all, out pos, out size);
            return;
        }

        // 1.5) The current zone has a fixed camera: glide to its view before swapping to it
        Camera fixedView = GetFixedCamera(currentZone);
        if (fixedView != null)
        {
            pos = new Vector3(fixedView.transform.position.x, fixedView.transform.position.y, initialPos.z);
            size = fixedView.orthographicSize;
            return;
        }

        // 2) Cerca del borde de la siguiente zona: alejar para verla
        if (nearNext)
        {
            Bounds b = zoneBounds[currentZone];
            b.Encapsulate(zoneBounds[currentZone + 1]);
            Fit(b, out pos, out size);
            return;
        }

        // 3) Normal: zoom inicial
        size = initialSize;
        if (currentZone == 0)
        {
            pos = initialPos;
            return;
        }

        posTime = followTime;
        pos = FollowClamped(zoneBounds[currentZone], size);
    }

    // Sigue al jugador sin salirse de los límites de la zona
    Vector3 FollowClamped(Bounds b, float size)
    {
        float h = size;
        float w = size * cam.aspect;
        Vector3 p = player.position;

        float x = b.size.x <= 2f * w ? b.center.x : Mathf.Clamp(p.x, b.min.x + w, b.max.x - w);
        float y = b.size.y <= 2f * h ? b.center.y : Mathf.Clamp(p.y, b.min.y + h, b.max.y - h);
        return new Vector3(x, y, initialPos.z);
    }

    void Fit(Bounds b, out Vector3 pos, out float size)
    {
        pos = new Vector3(b.center.x, b.center.y, initialPos.z);
        size = Mathf.Max(Mathf.Max(b.extents.y, b.extents.x / cam.aspect) + padding, initialSize);
    }

    Bounds CalcBounds(GameObject zone)
    {
        Bounds b = new Bounds();
        bool first = true;
        foreach (Renderer r in zone.GetComponentsInChildren<Renderer>())
        {
            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }
        return b;
    }

    List<Vector3Int> CollectCells(Tilemap map)
    {
        var list = new List<Vector3Int>();
        foreach (Vector3Int c in map.cellBounds.allPositionsWithin)
            if (map.HasTile(c)) list.Add(c);
        return list;
    }

    // Picks which camera should be on: the fixed one of the current zone, or the moving one
    void UpdateActiveCamera()
    {
        Camera fixedCam = GetFixedCamera(currentZone);

        // While a new zone is being revealed, use the moving camera so the zoom out still works
        Camera wanted = (fixedCam != null && revealTimer <= 0f) ? fixedCam : cam;

        // Going from one fixed camera to another: pass through the moving camera so it glides
        if (wanted != cam && ActiveCamera != null && ActiveCamera != cam && ActiveCamera != wanted)
            wanted = cam;

        // The moving camera is gliding toward a fixed view: swap only when it has arrived
        if (wanted != cam && ActiveCamera == cam && !HasArrived(wanted)) return;

        if (wanted == ActiveCamera) return;

        // The moving camera takes over: start from the fixed view to avoid a visual jump
        if (wanted == cam && ActiveCamera != null)
        {
            Vector3 p = ActiveCamera.transform.position;
            cam.transform.position = new Vector3(p.x, p.y, initialPos.z);
            cam.orthographicSize = ActiveCamera.orthographicSize;
            posVel = Vector3.zero;
            sizeVel = 0f;
        }

        // Turn on the wanted camera and turn off all the others
        wanted.gameObject.SetActive(true);
        if (cam != wanted) cam.gameObject.SetActive(false);
        foreach (Camera c in fixedCameras)
            if (c != null && c != wanted) c.gameObject.SetActive(false);

        ActiveCamera = wanted;
    }

    // True when the moving camera shows (almost) the same view as the fixed one
    bool HasArrived(Camera target)
    {
        Vector2 a = cam.transform.position;
        Vector2 b = target.transform.position;
        return Vector2.Distance(a, b) <= arriveTolerance &&
               Mathf.Abs(cam.orthographicSize - target.orthographicSize) <= arriveTolerance;
    }

    Camera GetFixedCamera(int zone)
    {
        if (fixedCameras == null || zone >= fixedCameras.Length) return null;
        return fixedCameras[zone];
    }
}
