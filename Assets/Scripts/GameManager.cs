using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Administrador global de la partida: lleva el puntaje (distancia) y las monedas,
/// controla el fin de la partida / reinicio y reproduce la música y los efectos de sonido.
/// Se accede a él mediante <see cref="Instancia"/> (patrón Singleton).
/// </summary>
/// <summary>Tipo de dinosaurio: se usa para elegir el sonido de muerte correspondiente.</summary>
public enum TipoDinosaurio { Triceratops, Velociraptor }

public class GameManager : MonoBehaviour
{
    /// <summary>Única instancia del GameManager en la escena.</summary>
    public static GameManager Instancia { get; private set; }

    [Header("Referencias")]
    /// <summary>Jugador de la partida.</summary>
    [SerializeField] private JugadorController jugador;
    /// <summary>Interfaz (contadores y menú de derrota).</summary>
    [SerializeField] private InterfazJuego interfaz;
    /// <summary>AudioSource para la música (fondo y derrota).</summary>
    [SerializeField] private AudioSource fuenteMusica;
    /// <summary>AudioSource para los efectos de sonido.</summary>
    [SerializeField] private AudioSource fuenteEfectos;

    [Header("Audio")]
    [SerializeField] private AudioClip musicaFondo;
    [SerializeField] private AudioClip musicaDerrota;
    [SerializeField] private AudioClip sonidoDisparo;
    [SerializeField] private AudioClip sonidoMoneda;

    [Header("Audio - Muerte por tipo de dinosaurio")]
    [SerializeField] private AudioClip[] clipsMuerteTriceratops;
    [SerializeField] private AudioClip[] clipsMuerteVelociraptor;
    [SerializeField, Range(0f, 2f)] private float volumenMuerte = 0.42f;

    /// <summary>Indica si la partida está en curso (false tras perder).</summary>
    public bool PartidaActiva { get; private set; }
    /// <summary>Monedas recolectadas en la partida actual.</summary>
    public int Monedas { get; private set; }
    /// <summary>Puntaje actual: metros recorridos por el jugador.</summary>
    public int Puntaje { get; private set; }

    /// <summary>Posición Z desde la que parte el jugador (para medir la distancia).</summary>
    private float posicionInicialZ;

    private void Awake()
    {
        Instancia = this;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        PartidaActiva = true;
        posicionInicialZ = jugador.transform.position.z;
        interfaz.OcultarMenuDerrota();
        interfaz.ActualizarMonedas(0);
        interfaz.ActualizarPuntaje(0);
        ReproducirMusica(musicaFondo, true);
    }

    private void Update()
    {
        if (!PartidaActiva)
        {
            return;
        }
        Puntaje = Mathf.FloorToInt(jugador.transform.position.z - posicionInicialZ);
        interfaz.ActualizarPuntaje(Puntaje);
    }

    /// <summary>Suma una moneda al contador y reproduce su sonido.</summary>
    public void AgregarMoneda()
    {
        Monedas++;
        interfaz.ActualizarMonedas(Monedas);
        ReproducirEfecto(sonidoMoneda, 0.5f);
    }

    /// <summary>Termina la partida: detiene al jugador, cambia la música y muestra el menú de derrota.</summary>
    public void TerminarPartida()
    {
        if (!PartidaActiva)
        {
            return;
        }
        PartidaActiva = false;
        jugador.Detener();
        ReproducirMusica(musicaDerrota, false);
        interfaz.MostrarMenuDerrota(Puntaje, Monedas);
        Time.timeScale = 0f;
    }

    /// <summary>Reinicia la partida recargando la escena (puntaje y monedas vuelven a cero).</summary>
    public void ReiniciarPartida()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>Reproduce el sonido de disparo del jugador.</summary>
    public void ReproducirDisparo()
    {
        ReproducirEfecto(sonidoDisparo, 0.9f);
    }

    /// <summary>Reproduce un sonido de muerte aleatorio del tipo de dinosaurio indicado.</summary>
    public void ReproducirMuerteDinosaurio(TipoDinosaurio tipo)
    {
        AudioClip[] clips = tipo == TipoDinosaurio.Triceratops ? clipsMuerteTriceratops : clipsMuerteVelociraptor;
        if (clips == null || clips.Length == 0)
        {
            return;
        }
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        ReproducirEfecto(clip, volumenMuerte);
    }

    private void ReproducirEfecto(AudioClip clip, float volumen)
    {
        if (clip != null && fuenteEfectos != null)
        {
            fuenteEfectos.PlayOneShot(clip, volumen);
        }
    }

    private void ReproducirMusica(AudioClip clip, bool enBucle)
    {
        if (clip == null || fuenteMusica == null)
        {
            return;
        }
        fuenteMusica.Stop();
        fuenteMusica.clip = clip;
        fuenteMusica.loop = enBucle;
        fuenteMusica.Play();
    }
}