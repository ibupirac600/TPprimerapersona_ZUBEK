using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class ConfigurarJuego
{
    private const string RutaPrefabEnemigo = "Assets/prefabs/enemigo.prefab";
    private const int EnemigosTotales = 5;

    [MenuItem("TP Zubek/Configurar todo")]
    public static void ConfigurarTodo()
    {
        var log = new List<string>();

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            EditorUtility.DisplayDialog("TP Zubek", "No encontré el PlayerCapsule (tag Player). Abrí la escena nvl1 y probá de nuevo.", "OK");
            return;
        }

        VidaJugador vida = jugador.GetComponent<VidaJugador>();
        if (vida == null)
        {
            vida = Undo.AddComponent<VidaJugador>(jugador);
            log.Add("VidaJugador agregado a " + jugador.name);
        }

        Arma arma = Object.FindAnyObjectByType<Arma>();
        PlayerInput entrada = jugador.GetComponent<PlayerInput>();

        AsegurarEventSystem(log);
        PrepararEnemigos(jugador.transform, log);

        Canvas canvas = CrearCanvas(log);
        TMP_Text textoVida = BuscarOCrearTexto(canvas.transform, "TextoVida", "Vida: 5 / 5", new Vector2(0, 1), new Vector2(30, -20), TextAlignmentOptions.TopLeft);
        TMP_Text textoTiempo = BuscarOCrearTexto(canvas.transform, "TextoTiempo", "Tiempo: 02:00", new Vector2(0.5f, 1), new Vector2(0, -20), TextAlignmentOptions.Top);
        TMP_Text textoEnemigos = BuscarOCrearTexto(canvas.transform, "TextoEnemigos", "Enemigos: 0 / 0", new Vector2(1, 1), new Vector2(-30, -20), TextAlignmentOptions.TopRight);

        GameObject gmObjeto = GameObject.Find("GameManager");
        if (gmObjeto == null)
        {
            gmObjeto = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(gmObjeto, "GameManager");
            log.Add("GameManager creado");
        }
        GameManager gm = gmObjeto.GetComponent<GameManager>();
        if (gm == null)
        {
            gm = Undo.AddComponent<GameManager>(gmObjeto);
        }

        GameObject panelVictoria = BuscarOCrearPanel(canvas.transform, "PanelVictoria", "¡GANASTE!", new Color(0f, 0.35f, 0f, 0.8f), gm);
        GameObject panelDerrota = BuscarOCrearPanel(canvas.transform, "PanelDerrota", "PERDISTE", new Color(0.4f, 0f, 0f, 0.8f), gm);

        var so = new SerializedObject(gm);
        so.FindProperty("vidaJugador").objectReferenceValue = vida;
        so.FindProperty("arma").objectReferenceValue = arma;
        so.FindProperty("entradaJugador").objectReferenceValue = entrada;
        so.FindProperty("textoVida").objectReferenceValue = textoVida;
        so.FindProperty("textoTiempo").objectReferenceValue = textoTiempo;
        so.FindProperty("textoEnemigos").objectReferenceValue = textoEnemigos;
        so.FindProperty("panelVictoria").objectReferenceValue = panelVictoria;
        so.FindProperty("panelDerrota").objectReferenceValue = panelDerrota;
        so.ApplyModifiedProperties();
        log.Add("Referencias del GameManager asignadas");

        AgregarEscenaABuild(log);

        var escena = EditorSceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        log.Add("Escena guardada");

        string resumen = string.Join("\n", log.Select(l => "• " + l));
        Debug.Log("[TP Zubek]\n" + resumen);
        EditorUtility.DisplayDialog("TP Zubek", "Listo:\n\n" + resumen, "OK");
    }

    private static void AsegurarEventSystem(List<string> log)
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(go, "EventSystem");
        log.Add("EventSystem creado");
    }

    private static void PrepararEnemigos(Transform jugador, List<string> log)
    {
        Enemigo[] enemigos = Object.FindObjectsByType<Enemigo>();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RutaPrefabEnemigo);

        if (prefab == null)
        {
            if (enemigos.Length == 0)
            {
                log.Add("ATENCIÓN: no hay ningún enemigo con el script Enemigo en la escena");
                return;
            }
            GameObject original = enemigos[0].gameObject;
            original.name = "Enemigo";
            prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(original, RutaPrefabEnemigo, InteractionMode.AutomatedAction);
            log.Add("Prefab creado en " + RutaPrefabEnemigo);
        }

        Vector3[] desplazamientos =
        {
            new Vector3(12, 0, 12), new Vector3(-12, 0, 12), new Vector3(12, 0, -12),
            new Vector3(-12, 0, -12), new Vector3(0, 0, 18), new Vector3(18, 0, 0)
        };

        int cantidad = enemigos.Length;
        int i = 0;
        while (cantidad < EnemigosTotales && i < desplazamientos.Length)
        {
            Vector3 punto = jugador.position + desplazamientos[i];
            i++;
            if (!NavMesh.SamplePosition(punto, out NavMeshHit hit, 6f, NavMesh.AllAreas))
            {
                continue;
            }
            var nuevo = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            nuevo.transform.position = hit.position;
            nuevo.transform.LookAt(new Vector3(jugador.position.x, hit.position.y, jugador.position.z));
            nuevo.name = "Enemigo " + (cantidad + 1);
            Undo.RegisterCreatedObjectUndo(nuevo, "Enemigo");
            cantidad++;
        }
        log.Add("Enemigos en la escena: " + cantidad);
    }

    private static Canvas CrearCanvas(List<string> log)
    {
        GameObject existente = GameObject.Find("HUD");
        if (existente != null && existente.GetComponent<Canvas>() != null)
        {
            return existente.GetComponent<Canvas>();
        }

        var go = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(go, "HUD");
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler escalador = go.GetComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920, 1080);
        escalador.matchWidthOrHeight = 0.5f;
        log.Add("Interfaz (HUD) creada");
        return canvas;
    }

    private static RectTransform CrearRect(string nombre, Transform padre)
    {
        var go = new GameObject(nombre, typeof(RectTransform));
        go.transform.SetParent(padre, false);
        return go.GetComponent<RectTransform>();
    }

    private static TMP_Text BuscarOCrearTexto(Transform padre, string nombre, string texto, Vector2 ancla, Vector2 posicion, TextAlignmentOptions alineacion)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null)
        {
            return existente.GetComponent<TMP_Text>();
        }

        RectTransform rt = CrearRect(nombre, padre);
        rt.anchorMin = ancla;
        rt.anchorMax = ancla;
        rt.pivot = ancla;
        rt.anchoredPosition = posicion;
        rt.sizeDelta = new Vector2(500, 70);

        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = texto;
        t.fontSize = 44;
        t.fontStyle = FontStyles.Bold;
        t.alignment = alineacion;
        t.color = Color.white;
        t.outlineWidth = 0.2f;
        t.outlineColor = Color.black;
        return t;
    }

    private static GameObject BuscarOCrearPanel(Transform padre, string nombre, string titulo, Color color, GameManager gm)
    {
        Transform existente = padre.Find(nombre);
        if (existente != null)
        {
            return existente.gameObject;
        }

        RectTransform panel = CrearRect(nombre, padre);
        panel.anchorMin = Vector2.zero;
        panel.anchorMax = Vector2.one;
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = color;

        RectTransform rtTitulo = CrearRect("Titulo", panel);
        rtTitulo.anchoredPosition = new Vector2(0, 100);
        rtTitulo.sizeDelta = new Vector2(1200, 200);
        var tTitulo = rtTitulo.gameObject.AddComponent<TextMeshProUGUI>();
        tTitulo.text = titulo;
        tTitulo.fontSize = 120;
        tTitulo.fontStyle = FontStyles.Bold;
        tTitulo.alignment = TextAlignmentOptions.Center;
        tTitulo.color = Color.white;

        RectTransform rtBoton = CrearRect("BotonReiniciar", panel);
        rtBoton.anchoredPosition = new Vector2(0, -100);
        rtBoton.sizeDelta = new Vector2(380, 100);
        rtBoton.gameObject.AddComponent<Image>().color = Color.white;
        Button boton = rtBoton.gameObject.AddComponent<Button>();
        UnityEventTools.AddPersistentListener(boton.onClick, gm.Reiniciar);

        RectTransform rtTextoBoton = CrearRect("Texto", rtBoton);
        rtTextoBoton.anchorMin = Vector2.zero;
        rtTextoBoton.anchorMax = Vector2.one;
        rtTextoBoton.offsetMin = Vector2.zero;
        rtTextoBoton.offsetMax = Vector2.zero;
        var tBoton = rtTextoBoton.gameObject.AddComponent<TextMeshProUGUI>();
        tBoton.text = "Reiniciar";
        tBoton.fontSize = 50;
        tBoton.alignment = TextAlignmentOptions.Center;
        tBoton.color = Color.black;

        panel.gameObject.SetActive(false);
        return panel.gameObject;
    }

    private static void AgregarEscenaABuild(List<string> log)
    {
        string ruta = EditorSceneManager.GetActiveScene().path;
        var escenas = EditorBuildSettings.scenes.ToList();
        if (escenas.Any(e => e.path == ruta))
        {
            return;
        }
        escenas.Insert(0, new EditorBuildSettingsScene(ruta, true));
        EditorBuildSettings.scenes = escenas.ToArray();
        log.Add("Escena agregada a la lista de build");
    }
}
