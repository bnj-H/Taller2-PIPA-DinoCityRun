using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Parte de la herramienta de construcción encargada de los prefabs de decoración, obstáculos,
/// enemigos, moneda y proyectil.
/// </summary>
public static partial class ConstructorDinoCityRun
{
    private static GameObject CrearPrefabEstatico(string nombre, string rutaFbx, float escala, string carpeta)
    {
        GameObject raiz = new GameObject(nombre);
        GameObject modelo = InstanciarModelo(rutaFbx);
        modelo.name = "Modelo";
        modelo.transform.SetParent(raiz.transform, false);
        modelo.transform.localScale = modelo.transform.localScale * escala;
        AplicarMaterial(modelo, matCiudad);
        return GuardarPrefab(raiz, carpeta);
    }

    private static void CrearPrefabsDecoracion()
    {
        AsegurarCarpetaPrefab("Decoracion");
        prefabsEdificio = "ABCDEFGH".Select(l => CrearPrefabEstatico("Edificio_" + l, CiudadFbx + "building_" + l + ".fbx", 4f, "Decoracion")).ToArray();
        string[] props = { "dumpster", "trash_A", "trash_B", "bush", "box_A", "box_B", "firehydrant", "bench" };
        prefabsEscombro = props.Select(p => CrearPrefabEstatico("Escombro_" + p, CiudadFbx + p + ".fbx", 2.5f, "Decoracion")).ToArray();
        Informe.AppendLine("Prefabs decoracion: " + prefabsEdificio.Length + " edificios, " + prefabsEscombro.Length + " escombros");
    }

    private static void CrearPrefabsVehiculos()
    {
        AsegurarCarpetaPrefab("Obstaculos");
        string[] autos = { "car_taxi", "car_police", "car_sedan", "car_hatchback", "car_stationwagon" };
        prefabsVehiculo = new GameObject[autos.Length];
        for (int i = 0; i < autos.Length; i++)
        {
            GameObject raiz = new GameObject("Vehiculo_" + autos[i].Replace("car_", ""));
            GameObject modelo = InstanciarModelo(CiudadFbx + autos[i] + ".fbx");
            modelo.name = "Modelo";
            modelo.transform.SetParent(raiz.transform, false);
            modelo.transform.localScale = modelo.transform.localScale * 3.6f;
            AplicarMaterial(modelo, matVehiculos[i]);
            Bounds b = Medir(modelo);
            BoxCollider caja = raiz.AddComponent<BoxCollider>();
            caja.isTrigger = true;
            caja.center = b.center;
            caja.size = b.size;
            raiz.AddComponent<VehiculoAbandonado>();
            prefabsVehiculo[i] = GuardarPrefab(raiz, "Obstaculos");
            Informe.AppendLine("Vehiculo " + autos[i] + " tamano " + b.size.ToString("F2"));
        }
    }

    private static GameObject CrearPrefabDinosaurio(string nombre, string fbx, string sufijoClip, float largoObjetivo, float velocidad)
    {
        GameObject raiz = new GameObject(nombre);
        GameObject modelo = InstanciarModelo(fbx);
        modelo.name = "Modelo";
        modelo.transform.SetParent(raiz.transform, false);
        modelo.transform.localRotation = Quaternion.Euler(0f, rotacionDinosaurios, 0f);
        Bounds b = Medir(modelo);
        modelo.transform.localScale = Vector3.one * (largoObjetivo / Mathf.Max(b.size.z, 0.0001f));
        b = Medir(modelo);
        modelo.transform.localPosition = new Vector3(-b.center.x, -b.min.y, -b.center.z);
        b = Medir(modelo);
        ConfigurarAnimador(modelo, "Anim_" + nombre, fbx, sufijoClip);
        BoxCollider caja = raiz.AddComponent<BoxCollider>();
        caja.isTrigger = true;
        caja.size = new Vector3(Mathf.Max(b.size.x, 1.2f), 8f, b.size.z);
        caja.center = new Vector3(0f, 4f, 0f);
        Dinosaurio dino = raiz.AddComponent<Dinosaurio>();
        AsignarFloat(dino, "velocidadMovimiento", velocidad);
        Informe.AppendLine(nombre + " tamano " + b.size.ToString("F2") + " velocidad " + velocidad);
        return GuardarPrefab(raiz, "Enemigos");
    }

    private static void CrearPrefabsDinosaurios()
    {
        AsegurarCarpetaPrefab("Enemigos");
        prefabsDinosaurio = new[]
        {
            CrearPrefabDinosaurio("Dino_Velociraptor", AnimadosFbx + "Velociraptor.fbx", "_Run", 4.5f, 6.5f),
            CrearPrefabDinosaurio("Dino_Triceratops", AnimadosFbx + "Triceratops.fbx", "_Run", 6f, 4f)
        };
    }

    private static void CrearPrefabsMonedaYProyectil()
    {
        AsegurarCarpetaPrefab("Objetos");

        GameObject moneda = new GameObject("Moneda");
        SphereCollider esfera = moneda.AddComponent<SphereCollider>();
        esfera.isTrigger = true;
        esfera.radius = 0.9f;
        moneda.AddComponent<Moneda>();
        GameObject disco = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disco.name = "Modelo";
        disco.transform.SetParent(moneda.transform, false);
        disco.transform.localScale = new Vector3(0.9f, 0.07f, 0.9f);
        disco.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        Object.DestroyImmediate(disco.GetComponent<Collider>());
        disco.GetComponent<Renderer>().sharedMaterial = matMoneda;
        prefabMoneda = GuardarPrefab(moneda, "Objetos");

        GameObject bala = new GameObject("Proyectil");
        Rigidbody cuerpo = bala.AddComponent<Rigidbody>();
        cuerpo.useGravity = false;
        SphereCollider colBala = bala.AddComponent<SphereCollider>();
        colBala.isTrigger = true;
        colBala.radius = 0.35f;
        bala.AddComponent<Proyectil>();
        GameObject esferaVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        esferaVisual.name = "Modelo";
        esferaVisual.transform.SetParent(bala.transform, false);
        esferaVisual.transform.localScale = Vector3.one * 0.28f;
        Object.DestroyImmediate(esferaVisual.GetComponent<Collider>());
        esferaVisual.GetComponent<Renderer>().sharedMaterial = matProyectil;
        prefabProyectil = GuardarPrefab(bala, "Objetos");
    }
}
