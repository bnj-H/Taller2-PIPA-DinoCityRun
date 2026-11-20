using UnityEngine;

/// <summary>
/// Plataforma de suelo del camino infinito. Al generarse crea como "hijos" suyos al menos un elemento
/// (moneda, dinosaurio o vehículo) en una de las 3 posiciones de carril, más decoración a los lados.
/// Cuando el jugador la deja atrás avisa al <see cref="GeneradorPlataformas"/> para que la destruya
/// y genere una nueva al final del camino. Al destruirse, sus hijos se destruyen con ella.
/// Su collider raíz es un Trigger que cubre toda la plataforma (detecta cuándo el jugador sale de ella).
/// </summary>
public class Plataforma : MonoBehaviour
{
    [Header("Posiciones de aparición (una por carril)")]
    /// <summary>Las 3 posiciones posibles (carril izquierdo, central y derecho).</summary>
    [SerializeField] private Transform[] posicionesCarril;

    [Header("Contenido")]
    /// <summary>Diseños de dinosaurio disponibles (se elige uno al azar).</summary>
    [SerializeField] private GameObject[] prefabsDinosaurio;
    /// <summary>Tipos de vehículo abandonado disponibles (se elige uno al azar).</summary>
    [SerializeField] private GameObject[] prefabsVehiculo;
    /// <summary>Prefab de la moneda.</summary>
    [SerializeField] private GameObject prefabMoneda;
    /// <summary>Probabilidad de que el elemento principal sea una moneda.</summary>
    [SerializeField, Range(0f, 1f)] private float probabilidadMoneda = 0.25f;
    /// <summary>Probabilidad de que el elemento principal sea un vehículo (el resto son dinosaurios).</summary>
    [SerializeField, Range(0f, 1f)] private float probabilidadVehiculo = 0.40f;
    /// <summary>Probabilidad de aparecer además una moneda extra en otro carril.</summary>
    [SerializeField, Range(0f, 1f)] private float probabilidadMonedaExtra = 0.6f;
    /// <summary>Altura a la que flotan las monedas.</summary>
    [SerializeField] private float alturaMoneda = 1.2f;
    /// <summary>Desplazamiento máximo en Z (respecto al centro) para repartir los elementos en la plataforma.</summary>
    [SerializeField] private float desplazamientoMaximoZ = 7f;

    [Header("Decoración de ciudad devastada")]
    [SerializeField] private GameObject[] prefabsEdificio;
    [SerializeField] private GameObject[] prefabsEscombro;
    /// <summary>Anclas a los lados del camino donde se colocan edificios.</summary>
    [SerializeField] private Transform[] anclasEdificio;
    /// <summary>Anclas junto a los muros donde se colocan escombros.</summary>
    [SerializeField] private Transform[] anclasEscombro;

    /// <summary>Generador que administra esta plataforma.</summary>
    private GeneradorPlataformas generador;
    /// <summary>Evita avisar más de una vez que el jugador la dejó atrás.</summary>
    private bool dejadaAtras;

    /// <summary>
    /// Prepara la plataforma recién creada: genera decoración y su contenido.
    /// </summary>
    /// <param name="generadorPlataformas">Generador dueño de la plataforma.</param>
    /// <param name="soloMonedas">Si es true solo aparecen monedas (plataformas iniciales seguras).</param>
    public void Inicializar(GeneradorPlataformas generadorPlataformas, bool soloMonedas)
    {
        generador = generadorPlataformas;
        GenerarDecoracion();
        GenerarContenido(soloMonedas);
    }

    /// <summary>Crea el elemento principal en un carril al azar y, a veces, una moneda extra en otro carril.</summary>
    private void GenerarContenido(bool soloMonedas)
    {
        int carrilPrincipal = Random.Range(0, posicionesCarril.Length);
        float azar = Random.value;
        float zAleatorio = Random.Range(-desplazamientoMaximoZ, desplazamientoMaximoZ);

        if (soloMonedas || azar < probabilidadMoneda)
        {
            Aparecer(prefabMoneda, carrilPrincipal, zAleatorio, alturaMoneda);
        }
        else if (azar < probabilidadMoneda + probabilidadVehiculo)
        {
            Aparecer(ElegirAlAzar(prefabsVehiculo), carrilPrincipal, zAleatorio, 0f);
        }
        else
        {
            // El dinosaurio aparece al fondo de la plataforma y avanza hacia el jugador.
            Aparecer(ElegirAlAzar(prefabsDinosaurio), carrilPrincipal, desplazamientoMaximoZ, 0f);
        }

        if (Random.value < probabilidadMonedaExtra)
        {
            int otroCarril = (carrilPrincipal + Random.Range(1, posicionesCarril.Length)) % posicionesCarril.Length;
            Aparecer(prefabMoneda, otroCarril, Random.Range(-desplazamientoMaximoZ, desplazamientoMaximoZ), alturaMoneda);
        }
    }

    /// <summary>Instancia un prefab como hijo de la plataforma en la posición del carril indicado.</summary>
    private void Aparecer(GameObject prefab, int carril, float desplazamientoZ, float altura)
    {
        if (prefab == null)
        {
            return;
        }
        Vector3 posicion = posicionesCarril[carril].position + new Vector3(0f, altura, desplazamientoZ);
        Instantiate(prefab, posicion, Quaternion.identity, transform);
    }

    /// <summary>Coloca edificios y escombros a los lados del camino (hijos de la plataforma).</summary>
    private void GenerarDecoracion()
    {
        foreach (Transform ancla in anclasEdificio)
        {
            GameObject prefab = ElegirAlAzar(prefabsEdificio);
            if (prefab == null) continue;
            float giro = ancla.localPosition.x < 0f ? 90f : -90f;
            GameObject edificio = Instantiate(prefab, ancla.position, Quaternion.Euler(0f, giro, 0f), transform);
            edificio.transform.localScale = new Vector3(1f, Random.Range(0.9f, 1.7f), 1f);
        }

        foreach (Transform ancla in anclasEscombro)
        {
            GameObject prefab = ElegirAlAzar(prefabsEscombro);
            if (prefab == null) continue;
            Vector3 posicion = ancla.position + new Vector3(0f, 0f, Random.Range(-4f, 4f));
            Instantiate(prefab, posicion, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
        }
    }

    private static GameObject ElegirAlAzar(GameObject[] opciones)
    {
        if (opciones == null || opciones.Length == 0) return null;
        return opciones[Random.Range(0, opciones.Length)];
    }

    /// <summary>
    /// Cuando el jugador sale de la zona de la plataforma (y ya está más adelante que su centro)
    /// se avisa al generador para destruirla y crear otra al final.
    /// </summary>
    /// <param name="otro">Collider que salió del trigger.</param>
    private void OnTriggerExit(Collider otro)
    {
        if (dejadaAtras || generador == null)
        {
            return;
        }
        JugadorController jugador = otro.GetComponentInParent<JugadorController>();
        if (jugador == null || jugador.transform.position.z <= transform.position.z)
        {
            return;
        }
        dejadaAtras = true;
        generador.PlataformaDejadaAtras(this);
    }
}