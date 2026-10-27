using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] Transform interactPoint;  // dónde se para el personaje (opcional)
    [SerializeField] bool oneShot = false;
    public UnityEvent onInteract;              // arrastra aquí lo que debe activarse

    bool used;

    public Vector2 Point => interactPoint ? interactPoint.position : transform.position;

    public void Interact()
    {
        if (oneShot && used) return;
        used = true;
        onInteract.Invoke();
    }
}
