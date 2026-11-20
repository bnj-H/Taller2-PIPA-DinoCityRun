using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Herramienta de editor que construye automáticamente el proyecto Dino City Run:
/// configura las animaciones, genera los audios, crea materiales, prefabs (jugador, dinosaurios,
/// vehículos, monedas, proyectiles, plataforma) y la escena de juego con su interfaz.
/// Se ejecuta desde el menú "DinoCityRun/Construir Proyecto".
/// </summary>
public static partial class ConstructorDinoCityRun
{
    private const string CiudadFbx = "Assets/ArchivosIniciales/AutosyCiudad/KayKit_City_Builder_Bits_1.0_FREE/Assets/fbx/";
    private const string PersonajesFbx = "Assets/ArchivosIniciales/ArmasyPersonajes/PersonajesyObstaculos/";
    private const string AnimadosFbx = "Assets/ArchivosIniciales/PersonajesAnimados/";
    private const string HeroeAnimado = AnimadosFbx + "Character_Hero@Walk With Rifle (1).fbx";
    private const float LargoPlataforma = 24f;
    private const int Frecuencia = 22050;

    /// <summary>Informe de lo construido (se devuelve al terminar).</summary>
    private static readonly StringBuilder Informe = new StringBuilder();

    [MenuItem("DinoCityRun/Construir Proyecto")]
    public static void ConstruirDesdeMenu()
    {
        Debug.Log(Construir());
    }

    /// <summary>Ejecuta todos los pasos de construcción y devuelve un informe de texto.</summary>
    public static string Construir()
    {
        Informe.Length = 0;
        AsegurarCarpetas();
        ConfigurarAnimaciones();
        GenerarAudios();
        CrearMateriales();
        CrearPrefabsDecoracion();
        CrearPrefabsVehiculos();
        CrearPrefabsDinosaurios();
        CrearPrefabsMonedaYProyectil();
        CrearPrefabPlataforma();
        CrearPrefabJugador();
        CrearEscena();
        AssetDatabase.SaveAssets();
        return Informe.ToString();
    }

    private static void AsegurarCarpetas()
    {
        foreach (string carpeta in new[] { "Prefabs", "Materials", "Audio", "Animations", "Scenes" })
        {
            if (!AssetDatabase.IsValidFolder("Assets/" + carpeta))
            {
                AssetDatabase.CreateFolder("Assets", carpeta);
            }
        }
    }

    /// <summary>Hace que las animaciones del héroe y de los dinosaurios se ejecuten en bucle infinito.</summary>
    private static void ConfigurarAnimaciones()
    {
        foreach (string ruta in new[] { HeroeAnimado, AnimadosFbx + "Velociraptor.fbx", AnimadosFbx + "Triceratops.fbx" })
        {
            ModelImporter importador = (ModelImporter)AssetImporter.GetAtPath(ruta);
            ModelImporterClipAnimation[] clips = importador.clipAnimations;
            if (clips == null || clips.Length == 0)
            {
                clips = importador.defaultClipAnimations;
            }
            foreach (ModelImporterClipAnimation clip in clips)
            {
                clip.loopTime = true;
            }
            importador.clipAnimations = clips;
            importador.SaveAndReimport();
        }
    }

    // ------------------------------------------------------------------ AUDIO (sintetizado)

    /// <summary>Genera con código los efectos y músicas (originales, sin derechos de autor) como archivos WAV.</summary>
    private static void GenerarAudios()
    {
        EscribirWav("Assets/Audio/Disparo.wav", CrearDisparo());
        EscribirWav("Assets/Audio/MuerteDinosaurio.wav", CrearMuerteDinosaurio());
        EscribirWav("Assets/Audio/Moneda.wav", CrearMoneda());
        EscribirWav("Assets/Audio/MusicaFondo.wav", CrearMusicaFondo());
        EscribirWav("Assets/Audio/MusicaDerrota.wav", CrearMusicaDerrota());
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Informe.AppendLine("Audios generados: 5");
    }

    private static float[] Buffer(float segundos)
    {
        return new float[(int)(segundos * Frecuencia)];
    }

    private static float Ruido(System.Random azar)
    {
        return (float)(azar.NextDouble() * 2.0 - 1.0);
    }

    private static float[] CrearDisparo()
    {
        float[] b = Buffer(0.3f);
        System.Random azar = new System.Random(1);
        float fase = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Frecuencia;
            fase += 2f * Mathf.PI * (60f + 260f * Mathf.Exp(-t * 25f)) / Frecuencia;
            float golpe = Mathf.Sin(fase) * Mathf.Exp(-t * 14f);
            float ruido = Ruido(azar) * Mathf.Exp(-t * 20f);
            b[i] = Mathf.Clamp(0.55f * golpe + 0.7f * ruido, -1f, 1f);
        }
        return b;
    }

    private static float[] CrearMuerteDinosaurio()
    {
        float[] b = Buffer(0.9f);
        System.Random azar = new System.Random(2);
        float fase = 0f;
        float filtro = 0f;
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Frecuencia;
            float frec = 70f + 200f * Mathf.Exp(-t * 2.2f) + 12f * Mathf.Sin(2f * Mathf.PI * 28f * t);
            fase += 2f * Mathf.PI * frec / Frecuencia;
            float ciclo = fase / (2f * Mathf.PI);
            float diente = 2f * (ciclo - Mathf.Floor(ciclo + 0.5f));
            float envolvente = Mathf.Min(1f, t / 0.04f) * Mathf.Exp(-t * 2.8f);
            float s = (0.7f * diente + 0.35f * Ruido(azar)) * envolvente;
            filtro += 0.25f * (s - filtro);
            b[i] = Mathf.Clamp(filtro * 1.8f, -1f, 1f);
        }
        return b;
    }

    private static float[] CrearMoneda()
    {
        float[] b = Buffer(0.3f);
        for (int i = 0; i < b.Length; i++)
        {
            float t = i / (float)Frecuencia;
            float frec = t < 0.08f ? 988f : 1319f;
            b[i] = 0.5f * Mathf.Sin(2f * Mathf.PI * frec * t) * Mathf.Exp(-t * 9f);
        }
        return b;
    }

    /// <summary>Suma una nota (0 = seno, 1 = cuadrada suave, 2 = triangular) con envolvente al buffer.</summary>
    private static void Nota(float[] b, float inicio, float duracion, float frec, float vol, int tipo)
    {
        int i0 = (int)(inicio * Frecuencia);
        int n = (int)(duracion * Frecuencia);
        for (int k = 0; k < n && i0 + k < b.Length; k++)
        {
            float t = k / (float)Frecuencia;
            float env = Mathf.Min(1f, t / 0.005f) * Mathf.Exp(-t * (4f / duracion));
            float x = 2f * Mathf.PI * frec * t;
            float onda;
            if (tipo == 0) onda = Mathf.Sin(x);
            else if (tipo == 1) onda = (Mathf.Sin(x) > 0f ? 0.6f : -0.6f);
            else onda = (2f / Mathf.PI) * Mathf.Asin(Mathf.Sin(x));
            b[i0 + k] += onda * env * vol;
        }
    }

    private static void Bombo(float[] b, float inicio)
    {
        int i0 = (int)(inicio * Frecuencia);
        float fase = 0f;
        for (int k = 0; k < (int)(0.25f * Frecuencia) && i0 + k < b.Length; k++)
        {
            float t = k / (float)Frecuencia;
            fase += 2f * Mathf.PI * (40f + 110f * Mathf.Exp(-t * 30f)) / Frecuencia;
            b[i0 + k] += Mathf.Sin(fase) * Mathf.Exp(-t * 14f) * 0.8f;
        }
    }

    private static void Caja(float[] b, float inicio, System.Random azar)
    {
        int i0 = (int)(inicio * Frecuencia);
        for (int k = 0; k < (int)(0.2f * Frecuencia) && i0 + k < b.Length; k++)
        {
            float t = k / (float)Frecuencia;
            b[i0 + k] += (Ruido(azar) * 0.4f + Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.3f) * Mathf.Exp(-t * 18f);
        }
    }

    private static void Hat(float[] b, float inicio, System.Random azar)
    {
        int i0 = (int)(inicio * Frecuencia);
        float anterior = 0f;
        for (int k = 0; k < (int)(0.06f * Frecuencia) && i0 + k < b.Length; k++)
        {
            float t = k / (float)Frecuencia;
            float r = Ruido(azar);
            b[i0 + k] += (r - anterior) * 0.12f * Mathf.Exp(-t * 70f);
            anterior = r;
        }
    }

    private static void Normalizar(float[] b, float maximo)
    {
        float pico = 0.0001f;
        foreach (float s in b) pico = Mathf.Max(pico, Mathf.Abs(s));
        for (int i = 0; i < b.Length; i++) b[i] = b[i] / pico * maximo;
    }

    /// <summary>Música de fondo en bucle: 8 compases a 120 BPM (La menor - Fa - Do - Sol) con bajo, arpegio y percusión.</summary>
    private static float[] CrearMusicaFondo()
    {
        float[] b = Buffer(16f);
        System.Random azar = new System.Random(3);
        float[] raices = { 110f, 110f, 87.31f, 87.31f, 130.81f, 130.81f, 98f, 98f };
        float[] am = { 220f, 261.63f, 329.63f };
        float[] fa = { 174.61f, 220f, 261.63f };
        float[] doM = { 261.63f, 329.63f, 392f };
        float[] sol = { 196f, 246.94f, 293.66f };
        float[][] acordes = { am, am, fa, fa, doM, doM, sol, sol };
        for (int compas = 0; compas < 8; compas++)
        {
            float t0 = compas * 2f;
            for (int corchea = 0; corchea < 8; corchea++)
            {
                float t = t0 + corchea * 0.25f;
                Nota(b, t, 0.22f, raices[compas] * (corchea % 4 == 3 ? 2f : 1f), 0.32f, 2);
                float[] ac = acordes[compas];
                Nota(b, t, 0.12f, ac[(corchea * 2) % 3] * 2f, 0.12f, 0);
                Nota(b, t + 0.125f, 0.12f, ac[(corchea * 2 + 1) % 3] * 2f, 0.12f, 0);
                Hat(b, t, azar);
            }
            Bombo(b, t0);
            Bombo(b, t0 + 1f);
            Caja(b, t0 + 0.5f, azar);
            Caja(b, t0 + 1.5f, azar);
        }
        Normalizar(b, 0.85f);
        return b;
    }

    /// <summary>Música triste de derrota: notas descendentes y acorde final en La menor.</summary>
    private static float[] CrearMusicaDerrota()
    {
        float[] b = Buffer(5f);
        float[] notas = { 440f, 392f, 349.23f, 329.63f };
        for (int n = 0; n < notas.Length; n++)
        {
            Nota(b, n * 0.8f, 1.4f, notas[n], 0.35f, 2);
            Nota(b, n * 0.8f, 1.4f, notas[n] / 2f, 0.2f, 0);
        }
        Nota(b, 3.2f, 1.8f, 110f, 0.3f, 2);
        Nota(b, 3.2f, 1.8f, 220f, 0.25f, 0);
        Nota(b, 3.2f, 1.8f, 261.63f, 0.2f, 0);
        Nota(b, 3.2f, 1.8f, 329.63f, 0.2f, 0);
        Normalizar(b, 0.8f);
        return b;
    }

    private static void EscribirWav(string ruta, float[] muestras)
    {
        using (FileStream fs = new FileStream(ruta, FileMode.Create))
        using (BinaryWriter w = new BinaryWriter(fs))
        {
            int bytes = muestras.Length * 2;
            w.Write(Encoding.ASCII.GetBytes("RIFF"));
            w.Write(36 + bytes);
            w.Write(Encoding.ASCII.GetBytes("WAVE"));
            w.Write(Encoding.ASCII.GetBytes("fmt "));
            w.Write(16);
            w.Write((short)1);
            w.Write((short)1);
            w.Write(Frecuencia);
            w.Write(Frecuencia * 2);
            w.Write((short)2);
            w.Write((short)16);
            w.Write(Encoding.ASCII.GetBytes("data"));
            w.Write(bytes);
            foreach (float s in muestras)
            {
                w.Write((short)(Mathf.Clamp(s, -1f, 1f) * 32000f));
            }
        }
    }
}