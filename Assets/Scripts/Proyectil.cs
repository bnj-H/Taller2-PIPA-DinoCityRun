using UnityEngine;

/// <summary>
/// Bala disparada por el jugador. Avanza en línea recta hacia adelante sin ser afectada por la gravedad
/// y se destruye al chocar con un dinosaurio u obstáculo (destruyéndolo también) o al agotar su tiempo de vida.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Proyectil : MonoBehaviour
{
    /// <summary>Velocidad de la bala en unidades/segundo.</summary>
    [SerializeField] private float velocidad = 45f;

    /// <summary>Segundos que la bala existe antes de autodestruirse si no choca con nada.</summary>
    [SerializeField] private float tiempoDeVida = 3f;

    /// <summary>Rigidbody que mueve la bala.</summary>
    private Rigidbody cuerpoRigido;

    private void Awake()
    {
        cuerpoRigido = GetComponent<Rigidbody>();
        cuerpoRigido.useGravity = false;
        cuerpoRigido.freezeRotation = true;
        cuerpoRigido.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        cuerpoRigido.velocity = transform.forward * velocidad;
        Destroy(gameObject, tiempoDeVida);
    }

    /// <summary>Si choca con un peligro (dinosaurio u obstáculo) ambos se destruyen.</summary>
    /// <param name="otro">Collider con el que chocó la bala.</param>
    private void OnTriggerEnter(Collider otro)
    {
        ObjetoPeligroso peligro = otro.GetComponentInParent<ObjetoPeligroso>();
        if (peligro == null)
        {
            return;
        }
        peligro.RecibirProyectil();
        Destroy(gameObject);
    }
}