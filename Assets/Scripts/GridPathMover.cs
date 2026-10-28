using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class GridPathMover : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Tilemap[] groundMaps;     // un Ground por zona (el 0 es la referencia de la rejilla)
    [SerializeField] Tilemap[] obstacleMaps;   // un Obstacles por zona
    Tilemap ground => groundMaps[0];
    [SerializeField] LayerMask interactableMask;
    [SerializeField] float speed = 4f;
    [SerializeField] Animator animator;   // parámetros float: MoveX, MoveY

    // Solo 4 direcciones: sin diagonales
    static readonly Vector3Int[] Dirs =
        { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    bool moving, cancel, hasClick;
    Vector3 clickWorld;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        // Centrar al personaje en su casilla
        transform.position = ground.GetCellCenterWorld(ground.WorldToCell(transform.position));
        SetAnimating(false);
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            clickWorld = cam.ScreenToWorldPoint(screenPos);
            hasClick = true;
        }

        if (!hasClick) return;

        if (moving) cancel = true;   // termina la casilla actual y atiende el nuevo clic
        else { hasClick = false; HandleClick(clickWorld); }
    }

    void HandleClick(Vector3 world)
    {
        Vector3Int start = ground.WorldToCell(transform.position);
        Vector3Int target = ground.WorldToCell(world);
        Interactable obj = GetInteractable(target);

        // Interactable: ir a una casilla adyacente. Casilla normal: ir a ella.
        List<Vector3Int> path = obj != null
            ? FindPath(start, c => Mathf.Abs(c.x - target.x) + Mathf.Abs(c.y - target.y) == 1)
            : FindPath(start, c => c == target);

        if (path != null) StartCoroutine(Follow(path, obj));
    }

    // BFS: camino más corto en casillas, solo 4 direcciones
    List<Vector3Int> FindPath(Vector3Int start, System.Func<Vector3Int, bool> isGoal)
    {
        var parent = new Dictionary<Vector3Int, Vector3Int> { [start] = start };
        var queue = new Queue<Vector3Int>();
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Vector3Int c = queue.Dequeue();
            if (isGoal(c))
            {
                var path = new List<Vector3Int>();
                while (c != start) { path.Add(c); c = parent[c]; }
                path.Reverse();
                return path;
            }

            foreach (Vector3Int d in Dirs)
            {
                Vector3Int n = c + d;
                if (parent.ContainsKey(n) || !IsWalkable(n)) continue;
                parent[n] = c;
                queue.Enqueue(n);
            }
        }
        return null; // inalcanzable
    }

    // Solo cuentan los tilemaps de zonas activas (desbloqueadas)
    bool IsWalkable(Vector3Int c)
    {
        bool onGround = false;
        foreach (Tilemap m in groundMaps)
            if (m.gameObject.activeInHierarchy && m.HasTile(c)) { onGround = true; break; }
        if (!onGround) return false;

        foreach (Tilemap m in obstacleMaps)
            if (m.gameObject.activeInHierarchy && m.HasTile(c)) return false;

        return GetInteractable(c) == null;
    }

    Interactable GetInteractable(Vector3Int cell)
    {
        Collider2D hit = Physics2D.OverlapPoint(ground.GetCellCenterWorld(cell), interactableMask);
        return hit != null && hit.TryGetComponent(out Interactable i) ? i : null;
    }

    IEnumerator Follow(List<Vector3Int> path, Interactable obj)
    {
        moving = true;
        cancel = false;

        foreach (Vector3Int cell in path)
        {
            if (cancel) break;

            Face(cell - ground.WorldToCell(transform.position));
            SetAnimating(true);

            Vector3 target = ground.GetCellCenterWorld(cell);
            while (transform.position != target)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                yield return null;
            }
        }

        bool arrived = !cancel;
        moving = false;
        cancel = false;
        SetAnimating(false);

        if (arrived && obj != null)
        {
            // Mirar hacia el objeto antes de interactuar
            Face(ground.WorldToCell(obj.transform.position) - ground.WorldToCell(transform.position));
            obj.Interact();
        }
    }

    void Face(Vector3Int dir)
    {
        if (animator == null || dir == Vector3Int.zero) return;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    // Animator.speed = 0 congela la animación cuando está quieto
    void SetAnimating(bool on)
    {
        if (animator != null) animator.speed = on ? 1f : 0f;
    }
}
