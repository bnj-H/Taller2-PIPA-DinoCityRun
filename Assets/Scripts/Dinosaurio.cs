using UnityEngine;

/// <summary>
/// Enemigo dinosaurio. Se mueve en línea recta hacia el jugador (eje -Z) sin cambiar de carril.
/// Cada diseño (Velociraptor, Triceratops) es un prefab distinto que solo varía en modelo,
/// tamaño y velocidad. Su collider es muy alto para que el jugador no pueda saltarlo por encima.
/// </summary>
public class Dinosaurio : ObjetoPeligroso
{
    /// <summary>Velocidad (unidades/segundo) con la que avanza hacia el jugador.</summary>
    [SerializeField] private float velocidadMovimiento = 5f;

    private Animator animator;
    private Collider miCollider;
    private bool estaMuerto = false;

    /// <summary>Tipo de dinosaurio: determina qué sonido de muerte se reproduce al morir.</summary>
    [SerializeField] private TipoDinosaurio tipoDinosaurio = TipoDinosaurio.Velociraptor;

    private void Start()
    {
        // Busca el Animator tanto en el padre como en sus hijos ("Modelo")
        animator = GetComponentInChildren<Animator>();
        miCollider = GetComponent<Collider>();
    }

    /// <summary>Avanza en línea recta en dirección contraria al jugador (hacia la pantalla).</summary>
    private void Update()
    {
        // Si ya está muerto, no se sigue moviendo
        if (estaMuerto) return;

        transform.Translate(Vector3.back * velocidadMovimiento * Time.deltaTime, Space.World);
    }

    /// <summary>Al morir por un proyectil reproduce el sonido de muerte, activa la animación y se destruye con retraso.</summary>
    public override void RecibirProyectil()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        // Instanciar las partículas rojas al recibir el disparo
        if (prefabParticulasImpacto != null)
        {
            Instantiate(prefabParticulasImpacto, transform.position + Vector3.up, Quaternion.identity);
        }

        if (GameManager.Instancia != null)
        {
            GameManager.Instancia.ReproducirMuerteDinosaurio(tipoDinosaurio);
        }

        if (animator != null)
        {
            animator.SetBool("isDeath", true);
        }

        if (miCollider != null)
        {
            miCollider.enabled = false;
        }

        Destroy(gameObject, 1.2f);
    }
}