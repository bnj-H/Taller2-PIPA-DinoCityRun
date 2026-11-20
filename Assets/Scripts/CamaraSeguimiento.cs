using UnityEngine;

/// <summary>
/// Cámara que sigue al jugador desde arriba y atrás. Solo se mueve hacia adelante junto con el jugador:
/// su posición horizontal (X) y su altura (Y) permanecen fijas.
/// </summary>
public class CamaraSeguimiento : MonoBehaviour
{
    /// <summary>Transform del jugador al que se sigue.</summary>
    [SerializeField] private Transform objetivo;
    /// <summary>Posición X fija de la cámara.</summary>
    [SerializeField] private float posicionX = 0f;
    /// <summary>Altura fija de la cámara.</summary>
    [SerializeField] private float altura = 5.5f;
    /// <summary>Distancia detrás del jugador (valor negativo en Z).</summary>
    [SerializeField] private float distanciaAtras = -9f;

    private void LateUpdate()
    {
        if (objetivo == null)
        {
            return;
        }
        transform.position = new Vector3(posicionX, altura, objetivo.position.z + distanciaAtras);
    }
}