using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] Collectible requires;   // si se asigna, solo funciona cuando ya fue recogido
    public UnityEvent onLocked;              // opcional: sonido o feedback de "aún no"

    [SerializeField] Transform interactPoint;  // dónde se para el personaje (opcional)
    [SerializeField] bool oneShot = false;
    public UnityEvent onInteract;              // arrastra aquí lo que debe activarse

    bool used;

    public Vector2 Point => interactPoint ? interactPoint.position : transform.position;

    public void Interact()
    {
        if (requires != null && !requires.Collected) { onLocked.Invoke(); return; }
        if (oneShot && used) return;
        used = true;
        onInteract.Invoke();
    }
}
