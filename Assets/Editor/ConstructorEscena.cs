using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Parte de la herramienta de construcción encargada del prefab de plataforma, del jugador
/// y de la escena de juego (cámara, luz, generador, interfaz y audio).
/// </summary>
public static partial class ConstructorDinoCityRun
{
    private static readonly Color ColorFondo = new Color(0.66f, 0.62f, 0.58f);

    private static GameObject Cubo(Transform padre, string nombre, Vector3 pos, Vector3 escala, Material mat, bool conservarCollider)
    {
        GameObject cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cubo.name = nombre;
        cubo.transform.SetParent(padre, false);
        cubo.transform.localPosition = pos;
        cubo.transform.localScale = escala;
        cubo.GetComponent<Renderer>().sharedMaterial = mat;
        if (!conservarCollider) Object.DestroyImmediate(cubo.GetComponent<Collider>());
        return cubo;
    }

    private static Transform Marcador(Transform padre, string nombre, Vector3 pos)
    {
        GameObject m = new GameObject(nombre);
        m.transform.SetParent(padre, false);
        m.transform.localPosition = pos;
        return m.transform;
    }

    private static void CrearPrefabPlataforma()
    {
        GameObject raiz = new GameObject("Plataforma");
        Transform t = raiz.transform;
        BoxCollider zona = raiz.AddComponent<BoxCollider>();
        zona.isTrigger = true;
        zona.size = new Vector3(14f, 20f, LargoPlataforma);
        zona.center = new Vector3(0f, 10f, 0f);

        Cubo(t, "Suelo", new Vector3(0f, -0.5f, 0f), new Vector3(10f, 1f, LargoPlataforma), matAsfalto, true);
        for (int i = 0; i < 4; i++)
        {
            float z = -9f + i * 6f;
            Cubo(t, "LineaCarril_Izq_" + i, new Vector3(-1.5f, 0.01f, z), new Vector3(0.15f, 0.02f, 3f), matLinea, false);
            Cubo(t, "LineaCarril_Der_" + i, new Vector3(1.5f, 0.01f, z), new Vector3(0.15f, 0.02f, 3f), matLinea, false);
        }
        Cubo(t, "Acera_Izq", new Vector3(-9.5f, 0f, 0f), new Vector3(9f, 0.3f, LargoPlataforma), matAcera, false);
        Cubo(t, "Acera_Der", new Vector3(9.5f, 0f, 0f), new Vector3(9f, 0.3f, LargoPlataforma), matAcera, false);
        Cubo(t, "Limite_Izq", new Vector3(-5.25f, 0.75f, 0f), new Vector3(0.5f, 1.5f, LargoPlataforma), matMuro, true);
        Cubo(t, "Limite_Der", new Vector3(5.25f, 0.75f, 0f), new Vector3(0.5f, 1.5f, LargoPlataforma), matMuro, true);

        Transform[] carriles =
        {
            Marcador(t, "Posicion_Carril_Izq", new Vector3(-3f, 0f, 0f)),
            Marcador(t, "Posicion_Carril_Centro", new Vector3(0f, 0f, 0f)),
            Marcador(t, "Posicion_Carril_Der", new Vector3(3f, 0f, 0f))
        };
        Transform[] anclasEdificio = new Transform[6];
        float[] zEdificios = { -8f, 0f, 8f };
        for (int i = 0; i < 3; i++)
        {
            anclasEdificio[i] = Marcador(t, "AnclaEdificio_Izq_" + i, new Vector3(-10.5f, 0f, zEdificios[i]));
            anclasEdificio[i + 3] = Marcador(t, "AnclaEdificio_Der_" + i, new Vector3(10.5f, 0f, zEdificios[i]));
        }
        Transform[] anclasEscombro =
        {
            Marcador(t, "AnclaEscombro_Izq_0", new Vector3(-6.4f, 0f, -6f)),
            Marcador(t, "AnclaEscombro_Izq_1", new Vector3(-6.4f, 0f, 6f)),
            Marcador(t, "AnclaEscombro_Der_0", new Vector3(6.4f, 0f, -6f)),
            Marcador(t, "AnclaEscombro_Der_1", new Vector3(6.4f, 0f, 6f))
        };

        Plataforma plataforma = raiz.AddComponent<Plataforma>();
        AsignarArray(plataforma, "posicionesCarril", carriles);
        AsignarArray(plataforma, "prefabsDinosaurio", prefabsDinosaurio);
        AsignarArray(plataforma, "prefabsVehiculo", prefabsVehiculo);
        Asignar(plataforma, "prefabMoneda", prefabMoneda);
        AsignarArray(plataforma, "prefabsEdificio", prefabsEdificio);
        AsignarArray(plataforma, "prefabsEscombro", prefabsEscombro);
        AsignarArray(plataforma, "anclasEdificio", anclasEdificio);
        AsignarArray(plataforma, "anclasEscombro", anclasEscombro);
        prefabPlataforma = GuardarPrefab(raiz, "Plataformas");
    }

    private static void CrearPrefabJugador()
    {
        GameObject raiz = new GameObject("Jugador");
        raiz.tag = "Player";
        GameObject modelo = InstanciarModelo(HeroeAnimado);
        modelo.name = "ModeloHeroe";
        modelo.transform.SetParent(raiz.transform, false);
        AplicarMaterial(modelo, matHeroe);
        Bounds b = Medir(modelo);
        modelo.transform.localScale = Vector3.one * (1.8f / Mathf.Max(b.size.y, 0.0001f));
        b = Medir(modelo);
        modelo.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
        ConfigurarAnimador(modelo, "Anim_Heroe", HeroeAnimado, "");

        Rigidbody cuerpo = raiz.AddComponent<Rigidbody>();
        cuerpo.isKinematic = true;
        cuerpo.useGravity = false;
        CapsuleCollider capsula = raiz.AddComponent<CapsuleCollider>();
        capsula.radius = 0.3f;
        capsula.height = 1.8f;
        capsula.center = new Vector3(0f, 0.9f, 0f);
        Transform punto = Marcador(raiz.transform, "PuntoDisparo", new Vector3(0.1f, 1.25f, 0.9f));
        JugadorController jugador = raiz.AddComponent<JugadorController>();
        Asignar(jugador, "prefabProyectil", prefabProyectil);
        Asignar(jugador, "puntoDisparo", punto);
        Informe.AppendLine("Heroe tamano " + b.size.ToString("F2"));
        prefabJugador = GuardarPrefab(raiz, "Jugador");
    }

    // ------------------------------------------------------------------ escena

    private static Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        Text texto = go.AddComponent<Text>();
        texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.fontStyle = FontStyle.Bold;
        texto.alignment = alineacion;
        texto.color = color;
        texto.horizontalOverflow = HorizontalWrapMode.Overflow;
        texto.verticalOverflow = VerticalWrapMode.Overflow;
        Outline contorno = go.AddComponent<Outline>();
        contorno.effectColor = Color.black;
        contorno.effectDistance = new Vector2(3f, -3f);
        return texto;
    }

    private static void Anclar(RectTransform rt, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano)
    {
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = pivote;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamano;
    }

    private static AudioSource CrearFuenteAudio(Transform padre, string nombre)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(padre, false);
        AudioSource fuente = go.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        return fuente;
    }

    private static AudioClip Clip(string nombre)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/" + nombre + ".wav");
    }

    private static void CrearEscena()
    {
        const string rutaEscena = "Assets/Scenes/DinoCityRun.unity";
        UnityEngine.SceneManagement.Scene escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.70f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = ColorFondo;
        RenderSettings.fogStartDistance = 70f;
        RenderSettings.fogEndDistance = 230f;

        GameObject luzObjeto = new GameObject("Luz Direccional");
        Light luz = luzObjeto.AddComponent<Light>();
        luz.type = LightType.Directional;
        luz.intensity = 1.1f;
        luz.shadows = LightShadows.Soft;
        luzObjeto.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        RenderSettings.sun = luz;

        GameObject jugadorObjeto = (GameObject)PrefabUtility.InstantiatePrefab(prefabJugador);
        JugadorController jugador = jugadorObjeto.GetComponent<JugadorController>();

        GameObject camObjeto = new GameObject("Main Camera");
        camObjeto.tag = "MainCamera";
        Camera cam = camObjeto.AddComponent<Camera>();
        camObjeto.AddComponent<AudioListener>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = ColorFondo;
        cam.fieldOfView = 60f;
        cam.farClipPlane = 400f;
        camObjeto.transform.position = new Vector3(0f, 5.5f, -9f);
        camObjeto.transform.rotation = Quaternion.Euler(17f, 0f, 0f);
        CamaraSeguimiento seguimiento = camObjeto.AddComponent<CamaraSeguimiento>();
        Asignar(seguimiento, "objetivo", jugadorObjeto.transform);

        GameObject genObjeto = new GameObject("GeneradorPlataformas");
        GeneradorPlataformas generador = genObjeto.AddComponent<GeneradorPlataformas>();
        Asignar(generador, "prefabPlataforma", prefabPlataforma);
        AsignarFloat(generador, "largoPlataforma", LargoPlataforma);

        // ---- Interfaz
        GameObject canvasObjeto = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObjeto.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler escalador = canvasObjeto.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);
        escalador.matchWidthOrHeight = 0.5f;
        Transform canvas = canvasObjeto.transform;

        Text textoPuntaje = CrearTexto(canvas, "TextoPuntaje", "PUNTUACIÓN: 00000000", 64, TextAnchor.UpperLeft, Color.white);
        Anclar(textoPuntaje.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(900f, 80f));
        Text textoMonedas = CrearTexto(canvas, "TextoMonedas", "MONEDAS: 00", 64, TextAnchor.UpperLeft, new Color(1f, 0.85f, 0.2f));
        Anclar(textoMonedas.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -110f), new Vector2(900f, 80f));

        GameObject panel = new GameObject("PanelDerrota", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas, false);
        RectTransform rtPanel = panel.GetComponent<RectTransform>();
        rtPanel.anchorMin = Vector2.zero;
        rtPanel.anchorMax = Vector2.one;
        rtPanel.offsetMin = Vector2.zero;
        rtPanel.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

        Text titulo = CrearTexto(panel.transform, "TextoTitulo", "¡HAS PERDIDO!", 130, TextAnchor.MiddleCenter, new Color(0.95f, 0.25f, 0.2f));
        Anclar(titulo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 230f), new Vector2(1400f, 200f));
        Text resumen = CrearTexto(panel.transform, "TextoResumen", "Puntuación: 0\nMonedas: 0", 64, TextAnchor.MiddleCenter, Color.white);
        Anclar(resumen.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(1200f, 220f));

        GameObject botonObjeto = new GameObject("BotonReiniciar", typeof(RectTransform), typeof(Image), typeof(Button), typeof(BotonInteractivo));
        botonObjeto.transform.SetParent(panel.transform, false);
        Anclar(botonObjeto.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -210f), new Vector2(560f, 150f));
        Button boton = botonObjeto.GetComponent<Button>();
        boton.transition = Selectable.Transition.None;
        Asignar(botonObjeto.GetComponent<BotonInteractivo>(), "imagenFondo", botonObjeto.GetComponent<Image>());
        Text textoBoton = CrearTexto(botonObjeto.transform, "Texto", "REINICIAR", 70, TextAnchor.MiddleCenter, Color.white);
        RectTransform rtTexto = textoBoton.rectTransform;
        rtTexto.anchorMin = Vector2.zero;
        rtTexto.anchorMax = Vector2.one;
        rtTexto.offsetMin = Vector2.zero;
        rtTexto.offsetMax = Vector2.zero;

        GameObject interfazObjeto = new GameObject("InterfazJuego");
        InterfazJuego interfaz = interfazObjeto.AddComponent<InterfazJuego>();
        Asignar(interfaz, "textoPuntaje", textoPuntaje);
        Asignar(interfaz, "textoMonedas", textoMonedas);
        Asignar(interfaz, "panelDerrota", panel);
        Asignar(interfaz, "textoResumenDerrota", resumen);

        GameObject gmObjeto = new GameObject("GameManager");
        GameManager gm = gmObjeto.AddComponent<GameManager>();
        Asignar(gm, "jugador", jugador);
        Asignar(gm, "interfaz", interfaz);
        Asignar(gm, "fuenteMusica", CrearFuenteAudio(gmObjeto.transform, "FuenteMusica"));
        Asignar(gm, "fuenteEfectos", CrearFuenteAudio(gmObjeto.transform, "FuenteEfectos"));
        Asignar(gm, "musicaFondo", Clip("MusicaFondo"));
        Asignar(gm, "musicaDerrota", Clip("MusicaDerrota"));
        Asignar(gm, "sonidoDisparo", Clip("Disparo"));
        AsignarArray(gm, "clipsMuerteTriceratops", new Object[] {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/TriceratopsDeath1.mp3"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/TriceratopsDeath2.mp3"),
        });
        AsignarArray(gm, "clipsMuerteVelociraptor", new Object[] {
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/VelociraptorDeath1.mp3"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/VelociraptorDeath2.mp3"),
            AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/VelociraptorDeath3.mp3"),
        });
        Asignar(gm, "sonidoMoneda", Clip("Moneda"));
        UnityEventTools.AddPersistentListener(boton.onClick, gm.ReiniciarPartida);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.productName = "Dino City Run";

        EditorSceneManager.SaveScene(escena, rutaEscena);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(rutaEscena, true) };
        AssetDatabase.DeleteAsset("Assets/Scenes/SampleScene.unity");
        Informe.AppendLine("Escena guardada: " + rutaEscena);
    }
}
