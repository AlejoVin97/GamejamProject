using UnityEngine;
using UnityEngine.Events;

// Objeto que el personaje automático recoge al pasar por encima
public class Collectible : MonoBehaviour
{
    [SerializeField] Transform walker;              // el personaje (el objeto con AutoWalker)
    [SerializeField] float pickupDistance = 0.4f;
    [SerializeField] bool resetOnLoop = false;      // false = se conserva aunque el personaje se pierda
    public UnityEvent onCollected;

    public bool Collected { get; private set; }

    void OnEnable() { AutoWalker.LoopRestarted += OnLoopRestarted; }
    void OnDisable() { AutoWalker.LoopRestarted -= OnLoopRestarted; }

    void Update()
    {
        if (Collected || walker == null) return;
        if (Vector2.Distance(walker.position, transform.position) <= pickupDistance)
        {
            Collected = true;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
            onCollected.Invoke();
        }
    }

    void OnLoopRestarted()
    {
        if (!resetOnLoop || !Collected) return;
        Collected = false;
        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = true;
    }
}