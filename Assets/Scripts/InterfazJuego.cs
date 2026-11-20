using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la interfaz de usuario: contador de puntaje y de monedas (siempre visibles en la parte superior)
/// y el menú de derrota con el botón para reiniciar.
/// </summary>
public class InterfazJuego : MonoBehaviour
{
    /// <summary>Texto del puntaje (distancia recorrida).</summary>
    [SerializeField] private Text textoPuntaje;
    /// <summary>Texto del contador de monedas.</summary>
    [SerializeField] private Text textoMonedas;
    /// <summary>Panel del menú de derrota (oculto mientras se juega).</summary>
    [SerializeField] private GameObject panelDerrota;
    /// <summary>Texto del menú de derrota con el resumen final.</summary>
    [SerializeField] private Text textoResumenDerrota;

    /// <summary>Actualiza el contador de puntaje con formato de 8 dígitos.</summary>
    /// <param name="puntaje">Puntaje actual.</param>
    public void ActualizarPuntaje(int puntaje)
    {
        textoPuntaje.text = "PUNTUACIÓN: " + puntaje.ToString("D8");
    }

    /// <summary>Actualiza el contador de monedas con formato de 2 dígitos.</summary>
    /// <param name="monedas">Cantidad de monedas recolectadas.</param>
    public void ActualizarMonedas(int monedas)
    {
        textoMonedas.text = "MONEDAS: " + monedas.ToString("D2");
    }

    /// <summary>Muestra el menú de derrota con el resumen de la partida.</summary>
    /// <param name="puntaje">Puntaje final.</param>
    /// <param name="monedas">Monedas finales.</param>
    public void MostrarMenuDerrota(int puntaje, int monedas)
    {
        textoResumenDerrota.text = "Puntuación: " + puntaje + "\nMonedas: " + monedas;
        panelDerrota.SetActive(true);
    }

    /// <summary>Oculta el menú de derrota.</summary>
    public void OcultarMenuDerrota()
    {
        panelDerrota.SetActive(false);
    }
}