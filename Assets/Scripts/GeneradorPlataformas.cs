using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera el camino infinito: al iniciar crea una serie de plataformas (entre 10 y 30) y, cada vez que
/// el jugador deja atrás una, la destruye y crea una nueva al final de la serie.
/// </summary>
public class GeneradorPlataformas : MonoBehaviour
{
    /// <summary>Prefab de la plataforma de suelo.</summary>
    [SerializeField] private GameObject prefabPlataforma;
    /// <summary>Cantidad de plataformas generadas al comenzar la partida (10 a 30).</summary>
    [SerializeField, Range(10, 30)] private int cantidadInicial = 15;
    /// <summary>Largo (eje Z) de cada plataforma; debe coincidir con el del prefab.</summary>
    [SerializeField] private float largoPlataforma = 24f;
    /// <summary>Plataformas iniciales que solo contienen monedas (zona segura de arranque).</summary>
    [SerializeField] private int plataformasInicialesSeguras = 2;
    /// <summary>Segundos que tarda en desaparecer una plataforma tras ser dejada atrás (para que no se note).</summary>
    [SerializeField] private float retrasoDestruccion = 1.5f;

    /// <summary>Plataformas activas, de la más antigua a la más nueva.</summary>
    private readonly List<Plataforma> plataformasActivas = new List<Plataforma>();
    /// <summary>Posición Z donde se creará la próxima plataforma.</summary>
    private float siguientePosicionZ;

    private void Start()
    {
        siguientePosicionZ = 0f;
        for (int i = 0; i < cantidadInicial; i++)
        {
            CrearPlataforma(i < plataformasInicialesSeguras);
        }
    }

    /// <summary>Crea una plataforma al final de la serie.</summary>
    /// <param name="soloMonedas">Si es true la plataforma solo contiene monedas.</param>
    private void CrearPlataforma(bool soloMonedas)
    {
        GameObject objeto = Instantiate(prefabPlataforma, new Vector3(0f, 0f, siguientePosicionZ), Quaternion.identity, transform);
        Plataforma plataforma = objeto.GetComponent<Plataforma>();
        plataforma.Inicializar(this, soloMonedas);
        plataformasActivas.Add(plataforma);
        siguientePosicionZ += largoPlataforma;
    }

    /// <summary>
    /// El jugador dejó atrás una plataforma: se destruye (con sus hijos) y se genera una nueva al final.
    /// </summary>
    /// <param name="plataforma">Plataforma que quedó atrás.</param>
    public void PlataformaDejadaAtras(Plataforma plataforma)
    {
        plataformasActivas.Remove(plataforma);
        Destroy(plataforma.gameObject, retrasoDestruccion);
        CrearPlataforma(false);
    }
}