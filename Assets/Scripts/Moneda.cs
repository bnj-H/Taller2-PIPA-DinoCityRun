using UnityEngine;

/// <summary>
/// Moneda recolectable. Gira sobre sí misma y, al ser tocada por el jugador,
/// aumenta el contador de monedas y desaparece.
/// </summary>
public class Moneda : MonoBehaviour
{
    /// <summary>Velocidad de giro en grados por segundo.</summary>
    [SerializeField] private float velocidadGiro = 180f;

    private void Update()
    {
        transform.Rotate(0f, velocidadGiro * Time.deltaTime, 0f, Space.World);
    }

    /// <summary>Si quien entra es el jugador, se suma una moneda y este objeto se destruye.</summary>
    /// <param name="otro">Collider que entró en el trigger de la moneda.</param>
    private void OnTriggerEnter(Collider otro)
    {
        if (otro.GetComponentInParent<JugadorController>() == null)
        {
            return;
        }
        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.AgregarMoneda();
        }
        Destroy(gameObject);
    }
}