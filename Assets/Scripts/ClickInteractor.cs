using UnityEngine;
using UnityEngine.InputSystem;

// El jugador ya no mueve al personaje: solo hace clic en objetos para activarlos
public class ClickInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] LayerMask interactableMask;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 world = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(world, interactableMask);

        if (hit != null && hit.TryGetComponent(out Interactable obj))
            obj.Interact();
    }
}
