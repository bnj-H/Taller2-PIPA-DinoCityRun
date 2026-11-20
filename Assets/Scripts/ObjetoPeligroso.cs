using UnityEngine;

/// <summary>
/// Clase base abstracta de todo elemento que mata al jugador al tocarlo (enemigos y obstáculos).
/// Define el comportamiento común: matar al jugador al colisionar y destruirse al recibir un proyectil.
/// Requiere un Collider configurado como Trigger en el mismo GameObject.
/// </summary>
public abstract class ObjetoPeligroso : MonoBehaviour
{
    [SerializeField] protected GameObject prefabParticulasImpacto;
    /// <summary>
    /// Se ejecuta cuando otro collider entra en la zona del peligro.
    /// Si es el jugador, se termina la partida.
    /// </summary>
    /// <param name="otro">Collider que entró en el trigger.</param>
    protected virtual void OnTriggerEnter(Collider otro)
    {
        JugadorController jugador = otro.GetComponentInParent<JugadorController>();
        if (jugador != null)
        {
            jugador.Morir();
        }
    }

    /// <summary>
    /// Llamado por el proyectil al impactar. Por defecto el objeto se destruye.
    /// Las clases hijas pueden extenderlo (por ejemplo, para reproducir un sonido).
    /// </summary>
    public virtual void RecibirProyectil()
    {
        if (prefabParticulasImpacto != null)
        {
            Instantiate(prefabParticulasImpacto, transform.position + Vector3.up, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}