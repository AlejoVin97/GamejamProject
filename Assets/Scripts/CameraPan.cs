
using UnityEngine;

public class CameraPan : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    [SerializeField] private float velocidad = 1f;
    [SerializeField] private float posicionInicialX = -10f;
    [SerializeField] private float posicionFinalX = 10f;

    private float posicionInicialY;
    private float posicionInicialZ;

    private void Start()
    {
        posicionInicialY = transform.position.y;
        posicionInicialZ = transform.position.z;

        transform.position = new Vector3(
            posicionInicialX,
            posicionInicialY,
            posicionInicialZ
        );
    }

    private void Update()
    {
        float nuevaPosicionX = Mathf.MoveTowards(
            transform.position.x,
            posicionFinalX,
            velocidad * Time.deltaTime
        );

        transform.position = new Vector3(
            nuevaPosicionX,
            posicionInicialY,
            posicionInicialZ
        );
    }
}
