using UnityEngine;

// Se agrega al objeto padre de una ruta para indicar qué pasa al terminarla
public class RouteInfo : MonoBehaviour
{
    public enum EndAction
    {
        Goal,      // ruta correcta: dispara onGoalReached
        Restart    // camino equivocado: vuelve al inicio del bucle
    }

    public EndAction onEnd = EndAction.Goal;
}
