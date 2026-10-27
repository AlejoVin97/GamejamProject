using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class AutoWalker : MonoBehaviour
{
    [Header("Mapa")]
    [SerializeField] Tilemap grid;              // cualquier tilemap de la rejilla (conversión de celdas)
    [SerializeField] Tilemap[] obstacleMaps;    // tilemaps que bloquean (árboles, etc.)
    [SerializeField] LayerMask blockerMask;     // capa de rocas/objetos que bloquean (Interactable)

    [Header("Ruta")]
    [SerializeField] Transform route;           // ruta principal: el waypoint 0 es el inicio del camino

    [Header("Movimiento")]
    [SerializeField] float speed = 3f;
    [SerializeField] float bumpWait = 1f;       // segundos que espera ante un bloqueo antes de perderse
    [SerializeField] int lostSteps = 3;         // casillas que se desvía hacia el costado
    [SerializeField] float lostPause = 0.6f;    // pausa al perderse, antes de reiniciar
    [SerializeField] float restartDelay = 0.8f; // pausa al aparecer en el inicio

    [SerializeField] Animator animator;         // parámetros float: MoveX, MoveY

    [Header("Eventos")]
    public UnityEvent onLost;                   // se perdió (ej. fundido a negro, sonido)
    public UnityEvent onRestart;                // vuelve al inicio del camino
    public UnityEvent onGoalReached;            // terminó una ruta correcta

    Coroutine run;
    bool lost;

    void Start()
    {
        run = StartCoroutine(Run());
    }

    // Cambia a otra ruta (el waypoint 0 de la nueva ruta debe ser donde termina la anterior)
    public void SetRoute(Transform newRoute)
    {
        route = newRoute;
        if (run != null) StopCoroutine(run);
        run = StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (true)
        {
            // Inicio (o reinicio) del camino
            transform.position = grid.GetCellCenterWorld(CellOf(route.GetChild(0)));
            SetAnimating(false);
            onRestart.Invoke();
            yield return new WaitForSeconds(restartDelay);

            lost = false;
            bool goal = false;
            Transform current = route;
            int w = 1;   // en la ruta principal el waypoint 0 es el inicio

            while (!lost)
            {
                // Fin de la ruta actual
                if (w >= current.childCount)
                {
                    if (current.TryGetComponent(out RouteInfo info) && info.onEnd == RouteInfo.EndAction.Restart)
                    {
                        // Camino equivocado: regresa al inicio
                        SetAnimating(false);
                        onLost.Invoke();
                        yield return new WaitForSeconds(lostPause);
                        lost = true;
                    }
                    else goal = true;
                    break;
                }

                Transform wp = current.GetChild(w);
                yield return WalkTo(CellOf(wp));
                if (lost) break;

                // Bifurcación: elige ruta según lo que haya en la casilla a revisar
                if (wp.TryGetComponent(out RouteBranch branch))
                {
                    Transform chosen = ChooseBranch(branch);
                    if (chosen != null)
                    {
                        current = chosen;
                        w = 0;      // en una ruta de bifurcación, todos los hijos son destinos
                        continue;
                    }
                }
                w++;
            }

            if (goal)
            {
                SetAnimating(false);
                onGoalReached.Invoke();
                yield break;
            }
            // Si se perdió, el while(true) lo regresa al inicio
        }
    }

    Transform ChooseBranch(RouteBranch b)
    {
        Vector3Int check = CurrentCell();

        if (b.checkPoint != null)
        {
            check = CellOf(b.checkPoint);
        }
        else if (b.routeIfFree != null && b.routeIfFree.childCount > 0)
        {
            // Por defecto: la casilla contigua en dirección a la ruta "libre"
            Vector3Int first = CellOf(b.routeIfFree.GetChild(0));
            if (first != CurrentCell()) check = CurrentCell() + NextDir(CurrentCell(), first);
        }

        bool blocked = IsBlocked(check);
        (blocked ? b.onBlocked : b.onFree).Invoke();
        return blocked ? b.routeIfBlocked : b.routeIfFree;
    }

    // Camina hasta la casilla; si algo la bloquea, se pierde (lost = true)
    IEnumerator WalkTo(Vector3Int target)
    {
        while (CurrentCell() != target)
        {
            Vector3Int dir = NextDir(CurrentCell(), target);
            Vector3Int next = CurrentCell() + dir;
            Face(dir);

            // Bloqueo: espera un poco por si el jugador lo despeja
            float waited = 0f;
            while (IsBlocked(next) && waited < bumpWait)
            {
                SetAnimating(false);
                waited += Time.deltaTime;
                yield return null;
            }

            if (IsBlocked(next))
            {
                yield return Lost(dir);
                lost = true;
                yield break;
            }

            yield return Step(next);
        }
    }

    // Se desvía hacia un costado (fuera del camino) y se pierde
    IEnumerator Lost(Vector3Int dir)
    {
        Vector3Int left = new Vector3Int(-dir.y, dir.x, 0);
        Vector3Int side = IsBlocked(CurrentCell() + left) ? -left : left;

        Face(side);
        for (int i = 0; i < lostSteps; i++)
        {
            Vector3Int next = CurrentCell() + side;
            if (IsBlocked(next)) break;
            yield return Step(next);
        }

        SetAnimating(false);
        onLost.Invoke();
        yield return new WaitForSeconds(lostPause);
    }

    IEnumerator Step(Vector3Int cell)
    {
        SetAnimating(true);
        Vector3 target = grid.GetCellCenterWorld(cell);
        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    // Primero en X, luego en Y: solo 4 direcciones
    Vector3Int NextDir(Vector3Int from, Vector3Int to)
    {
        if (from.x != to.x) return new Vector3Int((int)Mathf.Sign(to.x - from.x), 0, 0);
        return new Vector3Int(0, (int)Mathf.Sign(to.y - from.y), 0);
    }

    bool IsBlocked(Vector3Int cell)
    {
        foreach (Tilemap m in obstacleMaps)
            if (m.gameObject.activeInHierarchy && m.HasTile(cell)) return true;

        return Physics2D.OverlapPoint(grid.GetCellCenterWorld(cell), blockerMask) != null;
    }

    Vector3Int CurrentCell() => grid.WorldToCell(transform.position);
    Vector3Int CellOf(Transform t) => grid.WorldToCell(t.position);

    void Face(Vector3Int dir)
    {
        if (animator == null || dir == Vector3Int.zero) return;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    void SetAnimating(bool on)
    {
        if (animator != null) animator.speed = on ? 1f : 0f;
    }
}
