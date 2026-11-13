using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class DragInteractible : MonoBehaviour
{
    public enum Eje { Horizontal, Vertical }

    [Header("Recorrido en coordenadas de mundo")]
    [SerializeField] private Eje eje = Eje.Horizontal;
    [SerializeField] private float coordenadaMinima;
    [SerializeField] private float coordenadaMaxima = 5f;
    [SerializeField] private Camera camaraPrincipal;

    [Header("Suavizado opcional")]
    [SerializeField] private bool usarSuavizado;
    [SerializeField, Min(0f)] private float velocidadSuavizado = 12f;

    private Collider2D colisionador;
    private Rigidbody2D cuerpo;
    private Vector3 posicionOriginal;
    private float desfase;
    private float coordenadaObjetivo;
    private bool arrastrando;
    private bool movimientoPendiente;

    private void Awake()
    {
        colisionador = GetComponent<Collider2D>();
        cuerpo = GetComponent<Rigidbody2D>();
        posicionOriginal = transform.position;
        if (camaraPrincipal == null)
            camaraPrincipal = Camera.main;
    }

    private void Update()
    {
        Mouse raton = Mouse.current;
        if (raton == null || camaraPrincipal == null)
        {
            arrastrando = false;
            return;
        }

        // La profundidad es la distancia al plano del objeto, no su Z mundial.
        Vector2 pantalla = raton.position.ReadValue();
        float profundidad = Vector3.Dot(
            transform.position - camaraPrincipal.transform.position,
            camaraPrincipal.transform.forward);
        Vector3 puntoMundo = camaraPrincipal.ScreenToWorldPoint(
            new Vector3(pantalla.x, pantalla.y, profundidad));

        if (raton.leftButton.wasPressedThisFrame &&
            Physics2D.OverlapPoint(puntoMundo) == colisionador)
        {
            arrastrando = true;
            desfase = ObtenerCoordenada(transform.position) - ObtenerCoordenada(puntoMundo);
        }

        if (!arrastrando)
            return;

        if (!raton.leftButton.isPressed)
        {
            arrastrando = false;
            return;
        }

        coordenadaObjetivo = Mathf.Clamp(
            ObtenerCoordenada(puntoMundo) + desfase, coordenadaMinima, coordenadaMaxima);
        if (cuerpo != null)
            movimientoPendiente = true;
        else
            transform.position = CalcularPosicion(transform.position, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (cuerpo == null || !movimientoPendiente)
            return;

        Vector2 posicion = cuerpo.position;
        cuerpo.MovePosition(CalcularPosicion(
            new Vector3(posicion.x, posicion.y, posicionOriginal.z), Time.fixedDeltaTime));
        // Consume también la última posición solicitada antes de soltar.
        movimientoPendiente = arrastrando;
    }

    private float ObtenerCoordenada(Vector3 posicion)
    {
        return eje == Eje.Horizontal ? posicion.x : posicion.y;
    }

    private Vector3 CalcularPosicion(Vector3 actual, float intervalo)
    {
        float coordenada = coordenadaObjetivo;
        if (usarSuavizado)
            coordenada = Mathf.Lerp(ObtenerCoordenada(actual), coordenada,
                1f - Mathf.Exp(-velocidadSuavizado * intervalo));
        coordenada = Mathf.Clamp(coordenada, coordenadaMinima, coordenadaMaxima);

        // Conserva el otro eje y la profundidad originales.
        Vector3 posicion = posicionOriginal;
        if (eje == Eje.Horizontal)
            posicion.x = coordenada;
        else
            posicion.y = coordenada;
        return posicion;
    }

    private void OnDisable()
    {
        arrastrando = false;
        movimientoPendiente = false;
    }

    private void OnValidate()
    {
        if (coordenadaMinima > coordenadaMaxima)
        {
            float temporal = coordenadaMinima;
            coordenadaMinima = coordenadaMaxima;
            coordenadaMaxima = temporal;
        }
        velocidadSuavizado = Mathf.Max(0f, velocidadSuavizado);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 inicio = Application.isPlaying ? posicionOriginal : transform.position;
        Vector3 fin = inicio;
        if (eje == Eje.Horizontal)
        {
            inicio.x = coordenadaMinima;
            fin.x = coordenadaMaxima;
        }
        else
        {
            inicio.y = coordenadaMinima;
            fin.y = coordenadaMaxima;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(inicio, fin);
    }
}
