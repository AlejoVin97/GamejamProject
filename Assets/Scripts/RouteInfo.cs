using UnityEngine;

// Se agrega al objeto padre de una ruta para indicar qué pasa al terminarla
public class RouteInfo : MonoBehaviour
{
    public enum EndAction
    {
        Goal,      // ruta correcta: dispara onGoalReached
        Restart,   // camino equivocado: vuelve al inicio del bucle
        Loop,      // vuelve a su primer waypoint, indefinidamente
        Continue   // pasa directo a 'nextRoute' sin detenerse
    }

    public EndAction onEnd = EndAction.Goal;

    [Tooltip("Solo con Continue. Su primer waypoint debe estar donde termina esta ruta.")]
    public Transform nextRoute;

    [Tooltip("Solo con Continue. Si falla, reaparece al inicio de nextRoute (no de la zona 1).")]
    public bool nextBecomesStart = true;

    [Tooltip("Solo con Loop. Waypoint al que vuelve cada vuelta. Usa 1 si el waypoint 0 es solo el punto de entrada.")]
    public int loopStartIndex = 0;

    // Route chosen by the player (set by a bird click). Used only by Loop routes
    [HideInInspector] public Transform chosenRoute;

    void OnEnable() { AutoWalker.LoopRestarted += ClearChoice; }
    void OnDisable() { AutoWalker.LoopRestarted -= ClearChoice; }

    void ClearChoice() { chosenRoute = null; }

    // Call this from an Interactable's onInteract event, passing the route to take
    public void ChooseRoute(Transform route) { chosenRoute = route; }

    // Dibuja las rutas en la Scene para ver los bucles (cian = Loop)
    void OnDrawGizmos()
    {
        Gizmos.color = onEnd == EndAction.Loop ? Color.cyan : Color.yellow;
        int n = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Vector3 a = transform.GetChild(i).position;
            Gizmos.DrawWireSphere(a, 0.15f);
            if (i + 1 < n) Gizmos.DrawLine(a, transform.GetChild(i + 1).position);
            else if (onEnd == EndAction.Loop && n > 1) Gizmos.DrawLine(a, transform.GetChild(0).position);
        }
    }
}