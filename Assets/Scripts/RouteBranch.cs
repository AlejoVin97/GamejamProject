using UnityEngine;
using UnityEngine.Events;

// Se agrega a un waypoint. Al llegar, el personaje elige ruta según si una casilla está bloqueada.
public class RouteBranch : MonoBehaviour
{
    [Tooltip("Casilla a revisar. Si está vacío, se revisa la casilla contigua en dirección a 'Route If Free'.")]
    public Transform checkPoint;

    public Transform routeIfBlocked;   // casilla bloqueada (la roca sigue ahí): ruta correcta
    public Transform routeIfFree;      // casilla libre (la roca fue movida): ruta equivocada

    public UnityEvent onBlocked;       // ej. ZoneManager.UnlockNextZone()
    public UnityEvent onFree;
}
