using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Da interacción básica a un botón de menú: cambia de color y de tamaño al pasar el ratón por encima
/// y al presionarlo. Usa tiempo sin escala (unscaled) porque el juego queda en pausa en el menú de derrota.
/// </summary>
[RequireComponent(typeof(Button))]
public class BotonInteractivo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>Imagen de fondo del botón cuyo color se anima.</summary>
    [SerializeField] private Image imagenFondo;
    [SerializeField] private Color colorNormal = new Color(0.80f, 0.22f, 0.12f);
    [SerializeField] private Color colorSobre = new Color(1.00f, 0.50f, 0.15f);
    [SerializeField] private Color colorPresionado = new Color(0.55f, 0.12f, 0.08f);
    [SerializeField] private float escalaSobre = 1.1f;
    [SerializeField] private float escalaPresionado = 0.92f;
    /// <summary>Rapidez de la transición entre estados.</summary>
    [SerializeField] private float velocidadTransicion = 14f;

    private Vector3 escalaObjetivo = Vector3.one;
    private Color colorObjetivo;
    private bool ratonSobre;

    private void Awake()
    {
        if (imagenFondo == null)
        {
            imagenFondo = GetComponent<Image>();
        }
        colorObjetivo = colorNormal;
        imagenFondo.color = colorNormal;
    }

    private void Update()
    {
        float t = Time.unscaledDeltaTime * velocidadTransicion;
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, t);
        imagenFondo.color = Color.Lerp(imagenFondo.color, colorObjetivo, t);
    }

    public void OnPointerEnter(PointerEventData datos)
    {
        ratonSobre = true;
        escalaObjetivo = Vector3.one * escalaSobre;
        colorObjetivo = colorSobre;
    }

    public void OnPointerExit(PointerEventData datos)
    {
        ratonSobre = false;
        escalaObjetivo = Vector3.one;
        colorObjetivo = colorNormal;
    }

    public void OnPointerDown(PointerEventData datos)
    {
        escalaObjetivo = Vector3.one * escalaPresionado;
        colorObjetivo = colorPresionado;
    }

    public void OnPointerUp(PointerEventData datos)
    {
        escalaObjetivo = Vector3.one * (ratonSobre ? escalaSobre : 1f);
        colorObjetivo = ratonSobre ? colorSobre : colorNormal;
    }
}