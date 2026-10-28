using System.Collections;
using UnityEngine;

// Cada clic desliza la roca a la siguiente posición de la lista (y vuelve a la primera al terminar)
public class SlidableRock : MonoBehaviour
{
    [SerializeField] Vector2Int[] slideOffsets = { Vector2Int.zero, Vector2Int.right };  // en casillas, relativos al inicio
    [SerializeField] float slideTime = 0.15f;

    Vector3 startPos;
    int index;
    bool sliding;

    void Awake()
    {
        startPos = transform.position;
    }

    void OnEnable() { AutoWalker.LoopRestarted += ResetPosition; }
    void OnDisable() { AutoWalker.LoopRestarted -= ResetPosition; }

    // Vuelve a su posición inicial cuando el personaje reinicia el bucle
    void ResetPosition()
    {
        StopAllCoroutines();
        sliding = false;
        index = 0;
        transform.position = startPos;
        Physics2D.SyncTransforms();
    }

    // Llamar desde el UnityEvent onInteract del Interactable
    public void Slide()
    {
        if (sliding || slideOffsets.Length < 2) return;

        index = (index + 1) % slideOffsets.Length;
        Vector2Int o = slideOffsets[index];
        StartCoroutine(MoveTo(startPos + new Vector3(o.x, o.y, 0f)));
    }

    IEnumerator MoveTo(Vector3 target)
    {
        sliding = true;
        Vector3 from = transform.position;

        for (float t = 0; t < 1f; t += Time.deltaTime / slideTime)
        {
            transform.position = Vector3.Lerp(from, target, t);
            yield return null;
        }

        transform.position = target;
        Physics2D.SyncTransforms();   // el collider se actualiza de inmediato
        sliding = false;
    }
}
