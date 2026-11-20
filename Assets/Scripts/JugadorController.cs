using UnityEngine;

/// <summary>
/// Controla al jugador: avance automático hacia adelante (+Z), movimiento lateral con A/D,
/// salto con la barra espaciadora y disparo con F (mínimo 0,5 s entre disparos).
/// Usa un Rigidbody cinemático para que los Triggers de monedas y peligros detecten al jugador;
/// el movimiento, la gravedad y el salto se calculan manualmente.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class JugadorController : MonoBehaviour
{
    [Header("Avance")]
    /// <summary>Velocidad inicial de avance hacia adelante (unidades/segundo).</summary>
    [SerializeField] private float velocidadAvanceInicial = 12f;
    /// <summary>Velocidad máxima de avance.</summary>
    [SerializeField] private float velocidadAvanceMaxima = 18f;
    /// <summary>Aumento de velocidad por segundo (dificultad progresiva).</summary>
    [SerializeField] private float aceleracionAvance = 0.1f;

    [Header("Movimiento lateral")]
    /// <summary>Velocidad al moverse a izquierda/derecha con A y D.</summary>
    [SerializeField] private float velocidadLateral = 9f;
    /// <summary>Posición X máxima (en valor absoluto) para no salir del camino ni atravesar los muros.</summary>
    [SerializeField] private float limiteLateral = 4.3f;

    [Header("Salto")]
    /// <summary>Velocidad vertical inicial del salto.</summary>
    [SerializeField] private float velocidadSalto = 12.5f;
    /// <summary>Gravedad aplicada al jugador mientras está en el aire.</summary>
    [SerializeField] private float gravedad = 28f;

    [Header("Disparo")]
    /// <summary>Prefab de la bala que se dispara.</summary>
    [SerializeField] private GameObject prefabProyectil;
    /// <summary>Punto desde donde sale la bala.</summary>
    [SerializeField] private Transform puntoDisparo;
    /// <summary>Tiempo mínimo en segundos entre dos disparos.</summary>
    [SerializeField] private float tiempoEntreDisparos = 0.5f;

    [Header("Animación")]
    /// <summary>Multiplicador de velocidad de la animación de carrera.</summary>
    [SerializeField] private float velocidadAnimacion = 1.6f;

    /// <summary>Velocidad de avance actual (aumenta con el tiempo).</summary>
    public float VelocidadAvance { get; private set; }

    private float velocidadVertical;
    private bool enSuelo = true;
    private float tiempoUltimoDisparo = -999f;
    private bool activo = true;
    private Animator animador;

    private void Awake()
    {
        Rigidbody cuerpo = GetComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;
        VelocidadAvance = velocidadAvanceInicial;
        animador = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (animador != null)
        {
            animador.speed = velocidadAnimacion;
        }
    }

    private void Update()
    {
        if (!activo)
        {
            return;
        }

        float dt = Time.deltaTime;
        VelocidadAvance = Mathf.MoveTowards(VelocidadAvance, velocidadAvanceMaxima, aceleracionAvance * dt);

        float entradaHorizontal = 0f;
        if (Input.GetKey(KeyCode.A)) entradaHorizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) entradaHorizontal += 1f;

        Vector3 posicion = transform.position;
        posicion.z += VelocidadAvance * dt;
        posicion.x = Mathf.Clamp(posicion.x + entradaHorizontal * velocidadLateral * dt, -limiteLateral, limiteLateral);

        if (enSuelo && Input.GetKeyDown(KeyCode.Space))
        {
            velocidadVertical = velocidadSalto;
            enSuelo = false;
        }

        if (!enSuelo)
        {
            velocidadVertical -= gravedad * dt;
            posicion.y += velocidadVertical * dt;
            if (posicion.y <= 0f)
            {
                posicion.y = 0f;
                velocidadVertical = 0f;
                enSuelo = true;
            }
        }

        transform.position = posicion;

        if (Input.GetKeyDown(KeyCode.F) && Time.time - tiempoUltimoDisparo >= tiempoEntreDisparos)
        {
            Disparar();
        }
    }

    /// <summary>Instancia una bala hacia adelante y reproduce el sonido de disparo.</summary>
    private void Disparar()
    {
        tiempoUltimoDisparo = Time.time;
        Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirDisparo();
        }
    }

    /// <summary>Llamado por dinosaurios y obstáculos al tocar al jugador: termina la partida.</summary>
    public void Morir()
    {
        if (!activo || GameManager.Instancia == null)
        {
            return;
        }
        GameManager.Instancia.TerminarPartida();
    }

    /// <summary>Detiene el control del jugador y congela su animación (fin de la partida).</summary>
    public void Detener()
    {
        activo = false;
        if (animador != null)
        {
            animador.enabled = false;
        }
    }
}