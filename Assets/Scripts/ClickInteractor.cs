using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInteractor : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] ZoneManager zoneManager;   // gives us the camera that is active right now
    [SerializeField] LayerMask interactableMask;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        // Use the active camera of the zone, or the default one if there is no manager
        Camera current = (zoneManager != null && zoneManager.ActiveCamera != null) ? zoneManager.ActiveCamera : cam;

        Vector2 world = current.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D hit = Physics2D.OverlapPoint(world, interactableMask);

        if (hit != null && hit.TryGetComponent(out Interactable obj))
            obj.Interact();
    }
}