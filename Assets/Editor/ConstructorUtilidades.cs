using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Parte de la herramienta de construcción con materiales y utilidades comunes.
/// </summary>
public static partial class ConstructorDinoCityRun
{
    private static Material matAsfalto, matLinea, matAcera, matMuro, matMoneda, matProyectil, matHeroe, matCiudad;
    private static Material[] matVehiculos;
    private static GameObject[] prefabsEdificio, prefabsEscombro, prefabsVehiculo, prefabsDinosaurio;
    private static GameObject prefabMoneda, prefabProyectil, prefabPlataforma, prefabJugador;

    /// <summary>Giro (grados en Y) del modelo de los dinosaurios para que miren hacia el jugador (-Z).</summary>
    private static float rotacionDinosaurios = 180f;

    private static Material CrearMaterial(string nombre, Color color, Texture textura, float metalico, float suavidad, Color emision)
    {
        string ruta = "Assets/Materials/" + nombre + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(ruta);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, ruta);
        }
        mat.color = color;
        if (textura != null) mat.mainTexture = textura;
        mat.SetFloat("_Metallic", metalico);
        mat.SetFloat("_Glossiness", suavidad);
        if (emision != Color.black)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emision);
        }
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void CrearMateriales()
    {
        Texture texCiudad = AssetDatabase.LoadAssetAtPath<Texture2D>(CiudadFbx + "citybits_texture.png");
        Texture texHeroe = AssetDatabase.LoadAssetAtPath<Texture2D>(PersonajesFbx + "VoxelApocalypse_Character.png");
        matAsfalto = CrearMaterial("Mat_Asfalto", new Color(0.20f, 0.20f, 0.22f), null, 0f, 0.15f, Color.black);
        matLinea = CrearMaterial("Mat_LineaCarril", new Color(0.92f, 0.90f, 0.80f), null, 0f, 0.1f, Color.black);
        matAcera = CrearMaterial("Mat_Acera", new Color(0.45f, 0.43f, 0.40f), null, 0f, 0.1f, Color.black);
        matMuro = CrearMaterial("Mat_MuroLimite", new Color(0.55f, 0.35f, 0.25f), null, 0f, 0.1f, Color.black);
        matMoneda = CrearMaterial("Mat_Moneda", new Color(1f, 0.80f, 0.10f), null, 0.8f, 0.7f, new Color(0.35f, 0.25f, 0f));
        matProyectil = CrearMaterial("Mat_Proyectil", new Color(1f, 0.90f, 0.20f), null, 0f, 0.5f, new Color(1.5f, 1.1f, 0.2f));
        matHeroe = CrearMaterial("Mat_Heroe", Color.white, texHeroe, 0f, 0.1f, Color.black);
        matCiudad = CrearMaterial("Mat_CiudadRuinas", new Color(0.62f, 0.57f, 0.52f), texCiudad, 0f, 0.05f, Color.black);
        Color[] tintes =
        {
            new Color(0.75f, 0.70f, 0.45f), new Color(0.50f, 0.55f, 0.62f), new Color(0.60f, 0.45f, 0.42f),
            new Color(0.50f, 0.60f, 0.50f), new Color(0.62f, 0.60f, 0.55f)
        };
        matVehiculos = new Material[tintes.Length];
        for (int i = 0; i < tintes.Length; i++)
        {
            matVehiculos[i] = CrearMaterial("Mat_VehiculoAbandonado_" + (i + 1), tintes[i], texCiudad, 0f, 0.05f, Color.black);
        }
        Informe.AppendLine("Materiales creados");
    }

    private static GameObject InstanciarModelo(string rutaFbx)
    {
        return (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(rutaFbx));
    }

    private static Bounds Medir(GameObject objeto)
    {
        Renderer[] renders = objeto.GetComponentsInChildren<Renderer>();
        bool inicio = true;
        Bounds b = new Bounds();
        foreach (Renderer r in renders)
        {
            if (r is SkinnedMeshRenderer)
            {
                if (inicio) { b = r.bounds; inicio = false; } else b.Encapsulate(r.bounds);
                continue;
            }
            Bounds local = r.localBounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 esquina = local.center + Vector3.Scale(local.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 mundo = r.transform.TransformPoint(esquina);
                if (inicio) { b = new Bounds(mundo, Vector3.zero); inicio = false; } else b.Encapsulate(mundo);
            }
        }
        return b;
    }

    private static void AplicarMaterial(GameObject objeto, Material material)
    {
        foreach (Renderer r in objeto.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[r.sharedMaterials.Length];
            for (int i = 0; i < mats.Length; i++) mats[i] = material;
            r.sharedMaterials = mats;
        }
    }

    private static GameObject GuardarPrefab(GameObject raiz, string carpeta)
    {
        AsegurarCarpetaPrefab(carpeta);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, "Assets/Prefabs/" + carpeta + "/" + raiz.name + ".prefab");
        Object.DestroyImmediate(raiz);
        return prefab;
    }

    private static void AsegurarCarpetaPrefab(string nombre)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/" + nombre)) AssetDatabase.CreateFolder("Assets/Prefabs", nombre);
    }

    private static void Asignar(Object destino, string campo, Object valor)
    {
        SerializedObject so = new SerializedObject(destino);
        so.FindProperty(campo).objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AsignarArray(Object destino, string campo, Object[] valores)
    {
        SerializedObject so = new SerializedObject(destino);
        SerializedProperty p = so.FindProperty(campo);
        p.arraySize = valores.Length;
        for (int i = 0; i < valores.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = valores[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AsignarFloat(Object destino, string campo, float valor)
    {
        SerializedObject so = new SerializedObject(destino);
        so.FindProperty(campo).floatValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AnimatorController CrearControlador(string nombre, AnimationClip clip)
    {
        string ruta = "Assets/Animations/" + nombre + ".controller";
        if (AssetDatabase.LoadAssetAtPath<Object>(ruta) != null) AssetDatabase.DeleteAsset(ruta);
        AnimatorController controlador = AnimatorController.CreateAnimatorControllerAtPath(ruta);
        AnimatorState estado = controlador.layers[0].stateMachine.AddState("Correr");
        estado.motion = clip;
        controlador.layers[0].stateMachine.defaultState = estado;
        return controlador;
    }

    private static AnimationClip BuscarClip(string rutaFbx, string sufijo)
    {
        return AssetDatabase.LoadAllAssetsAtPath(rutaFbx).OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview__") && c.name.EndsWith(sufijo));
    }

    private static void ConfigurarAnimador(GameObject modelo, string nombreControlador, string rutaFbx, string sufijoClip)
    {
        Animator animador = modelo.GetComponent<Animator>();
        if (animador == null) animador = modelo.AddComponent<Animator>();
        animador.runtimeAnimatorController = CrearControlador(nombreControlador, BuscarClip(rutaFbx, sufijoClip));
        animador.applyRootMotion = false;
        foreach (SkinnedMeshRenderer r in modelo.GetComponentsInChildren<SkinnedMeshRenderer>()) r.updateWhenOffscreen = true;
    }
}
