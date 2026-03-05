using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using TMPro;

/// <summary>
/// Editor script: FPTSim/Setup FlappyVocab Minigame
/// Tạo toàn bộ scene FlappyVocab từ đầu và setup integration với FPTSim.
/// </summary>
public static class FlappyVocabSetup
{
    private const string GameScenePath   = "Assets/MiniGames/FlappyVocabGame/Scenes/FlappyVocab.unity";
    private const string IntegScenePath  = "Assets/Scenes/Minigame_FlappyVocab.unity";
    private const string PrefabFolder    = "Assets/MiniGames/FlappyVocabGame/Prefabs";
    private const string WordPrefabPath  = "Assets/MiniGames/FlappyVocabGame/Prefabs/VocabWord.prefab";

    // Sprite asset paths
    private const string SpritesBase = "Assets/MiniGames/FlappyVocabGame/Sprites/Flappy Bird Assets 1.6 (Zip)/Flappy Bird Assets/";
    private const string BgSpritePath   = SpritesBase + "Background/Background1.png";
    private static readonly string[] BirdFramePaths = new[]
    {
        SpritesBase + "Player/StyleBird1/Bird1-1.png",
        SpritesBase + "Player/StyleBird1/Bird1-2.png",
        SpritesBase + "Player/StyleBird1/Bird1-3.png",
        SpritesBase + "Player/StyleBird1/Bird1-4.png",
        SpritesBase + "Player/StyleBird1/Bird1-5.png",
        SpritesBase + "Player/StyleBird1/Bird1-6.png",
        SpritesBase + "Player/StyleBird1/Bird1-7.png",
    };

    [MenuItem("FPTSim/Setup FlappyVocab Minigame")]
    public static void SetupAll()
    {
        EnsureFolders();
        CreateWordPrefab();
        CreateGameScene();
        CreateIntegrationScene();
        Debug.Log("[FlappyVocabSetup] ✅ FlappyVocab setup hoàn tất!");
    }

    // ─── Folders ─────────────────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        foreach (string dir in new[]
        {
            "Assets/MiniGames/FlappyVocabGame",
            "Assets/MiniGames/FlappyVocabGame/Scenes",
            "Assets/MiniGames/FlappyVocabGame/Scripts",
            PrefabFolder
        })
        {
            string[] parts = dir.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }

    // ─── Word Prefab ─────────────────────────────────────────────────────────

    private static void CreateWordPrefab()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(WordPrefabPath) != null)
        {
            Debug.Log("[FlappyVocabSetup] VocabWord.prefab đã tồn tại, bỏ qua.");
            return;
        }

        // Root GO
        var root = new GameObject("VocabWord");

        // Background (rounded rect via Sprite)
        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(root.transform, false);
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = GetOrCreateRoundedSprite();
        bgSr.color  = new Color(1f, 1f, 1f, 0.15f);
        bgGo.transform.localScale = new Vector3(2.8f, 0.7f, 1f);

        // Text (TMP)
        var textGo = new GameObject("WordLabel");
        textGo.transform.SetParent(root.transform, false);
        var tmp = textGo.AddComponent<TextMeshPro>();
        tmp.text         = "WORD";
        tmp.fontSize     = 4.5f;
        tmp.alignment    = TextAlignmentOptions.Center;
        tmp.color        = Color.white;
        tmp.fontStyle    = FontStyles.Bold;
        textGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        // Collider (trigger)
        var col = root.AddComponent<BoxCollider2D>();
        col.size      = new Vector2(2.5f, 0.65f);
        col.isTrigger = true;

        // VocabWordBehaviour
        var behaviour = root.AddComponent<FlappyVocabGame.VocabWordBehaviour>();
        SerializedObject so = new SerializedObject(behaviour);
        so.FindProperty("label").objectReferenceValue = tmp;
        so.FindProperty("background").objectReferenceValue = bgSr;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Save as prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, WordPrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[FlappyVocabSetup] VocabWord prefab → {WordPrefabPath}");
    }

    private static Sprite GetOrCreateRoundedSprite()
    {
        // Dùng Unity built-in sprite nếu có, fallback về null
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    // ─── Game Scene ───────────────────────────────────────────────────────────

    private static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.53f, 0.81f, 0.98f); // light sky blue
        cam.transform.position = new Vector3(0f, 0f, -10f);
        camGo.AddComponent<AudioListener>();

        // ── Background gradient (simple quad) ──
        CreateBackground();

        // ── Bird ──
        var birdGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        birdGo.name = "Bird";
        Object.DestroyImmediate(birdGo.GetComponent<Collider>());  // remove 3D collider
        Object.DestroyImmediate(birdGo.GetComponent<MeshRenderer>());
        Object.DestroyImmediate(birdGo.GetComponent<MeshFilter>());

        // Use SpriteRenderer instead for 2D
        var birdSr = birdGo.AddComponent<SpriteRenderer>();
        birdSr.color = new Color(1f, 0.9f, 0.1f); // yellow bird
        birdGo.transform.position = new Vector3(-4f, 0f, 0f);
        birdGo.transform.localScale = Vector3.one * 0.6f;

        var birdRb = birdGo.AddComponent<Rigidbody2D>();
        birdRb.gravityScale = 2f;
        birdRb.constraints  = RigidbodyConstraints2D.FreezeRotation;

        var birdCol = birdGo.AddComponent<CircleCollider2D>();
        birdCol.radius    = 0.3f;
        birdCol.isTrigger = true;

        // ── Word Spawner ──
        var spawnerGo = new GameObject("WordSpawner");
        spawnerGo.transform.position = new Vector3(9f, 0f, 0f);
        var spawner = spawnerGo.AddComponent<FlappyVocabGame.WordSpawner>();

        // ── Game Manager ──
        var mgrGo = new GameObject("[FlappyVocabGameManager]");
        var manager = mgrGo.AddComponent<FlappyVocabGame.FlappyVocabGameManager>();

        // ── Canvas + UI ──
        BuildUI(mgrGo, manager, spawner, birdGo);

        // ── Assign bird controller (needs references) ──
        var birdCtrl = birdGo.AddComponent<FlappyVocabGame.BirdController>();
        SerializedObject birdSo = new SerializedObject(birdCtrl);
        birdSo.FindProperty("gameManager").objectReferenceValue = manager;
        birdSo.FindProperty("wordSpawner").objectReferenceValue = spawner;
        birdSo.ApplyModifiedPropertiesWithoutUndo();

        // ── Assign manager references ──
        SerializedObject mgrSo = new SerializedObject(manager);
        mgrSo.FindProperty("bird").objectReferenceValue    = birdCtrl;
        mgrSo.FindProperty("wordSpawner").objectReferenceValue = spawner;
        mgrSo.ApplyModifiedPropertiesWithoutUndo();

        // Assign word prefab to spawner
        var wordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(WordPrefabPath);
        SerializedObject spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("wordPrefab").objectReferenceValue = wordPrefab;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.SaveScene(scene, GameScenePath);
        Debug.Log($"[FlappyVocabSetup] Game scene → {GameScenePath}");
    }

    private static void CreateBackground()
    {
        var bgGo  = new GameObject("Background");
        var bgSr  = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sortingOrder = -10;
        bgSr.color        = new Color(0.4f, 0.75f, 1f);
        bgGo.transform.position   = new Vector3(0f, 0f, 1f);
        bgGo.transform.localScale = new Vector3(25f, 12f, 1f);

        // Create minimal 1x1 white sprite
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        bgSr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    private static void BuildUI(GameObject mgrGo,
        FlappyVocabGame.FlappyVocabGameManager manager,
        FlappyVocabGame.WordSpawner spawner,
        GameObject bird)
    {
        // Canvas
        var canvasGo = new GameObject("Canvas");
        var canvas   = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Question Panel (top center)
        var panelGo = new GameObject("QuestionPanel");
        panelGo.transform.SetParent(canvasGo.transform, false);
        var panelImg = panelGo.AddComponent<UnityEngine.UI.Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.5f);
        SetRectAnchored(panelGo, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -120f), new Vector2(0f, 0f), new Vector2(0f, 120f));

        var questionTextGo = CreateTMPText(panelGo, "QuestionText", "Translate: CON MÈO", 28f);
        SetRectAnchored(questionTextGo, Vector2.zero, Vector2.one,
            new Vector2(10f, 10f), new Vector2(-10f, -10f), Vector2.zero);

        // Score (top right)
        var scoreGo = CreateTMPText(canvasGo, "ScoreText", "Score: 0", 22f);
        SetRectAnchored(scoreGo, new Vector2(1f, 1f), new Vector2(1f, 1f),
            new Vector2(-200f, -130f), new Vector2(0f, -10f), Vector2.zero);
        scoreGo.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopRight;

        // Lives (top left)
        var livesGo = CreateTMPText(canvasGo, "LivesText", "Lives: 3", 22f);
        SetRectAnchored(livesGo, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(10f, -130f), new Vector2(210f, -10f), Vector2.zero);
        livesGo.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.TopLeft;

        // Timer (top center)
        var timerGo = CreateTMPText(canvasGo, "TimerText", "60", 24f);
        SetRectAnchored(timerGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-60f, -130f), new Vector2(60f, -10f), Vector2.zero);
        timerGo.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Top;

        // GameOver Panel (hidden by default)
        var goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(canvasGo.transform, false);
        var goPanelImg = goPanel.AddComponent<UnityEngine.UI.Image>();
        goPanelImg.color = new Color(0f, 0f, 0f, 0.75f);
        SetRectAnchored(goPanel, Vector2.zero, Vector2.one,
            Vector2.zero, Vector2.zero, Vector2.zero);
        goPanel.SetActive(false);

        var finalScoreGo = CreateTMPText(goPanel, "FinalScoreText", "Score: 0", 40f);
        SetRectAnchored(finalScoreGo, Vector2.zero, Vector2.one,
            new Vector2(0f, 50f), new Vector2(0f, -50f), Vector2.zero);
        finalScoreGo.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Center;

        // Assign UI refs to manager
        SerializedObject so = new SerializedObject(manager);
        so.FindProperty("scoreText").objectReferenceValue    = scoreGo.GetComponent<TMP_Text>();
        so.FindProperty("livesText").objectReferenceValue    = livesGo.GetComponent<TMP_Text>();
        so.FindProperty("timerText").objectReferenceValue    = timerGo.GetComponent<TMP_Text>();
        so.FindProperty("gameOverPanel").objectReferenceValue = goPanel;
        so.FindProperty("finalScoreText").objectReferenceValue = finalScoreGo.GetComponent<TMP_Text>();
        so.ApplyModifiedPropertiesWithoutUndo();

        // Assign question text to spawner
        SerializedObject spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("questionText").objectReferenceValue = questionTextGo.GetComponent<TMP_Text>();
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject CreateTMPText(GameObject parent, string name, string text, float fontSize)
    {
        var go  = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text        = text;
        tmp.fontSize    = fontSize;
        tmp.color       = Color.white;
        tmp.fontStyle   = FontStyles.Bold;
        return go;
    }

    private static void SetRectAnchored(GameObject go,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax,
        Vector2 pivot)
    {
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) rt = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
        if (pivot != Vector2.zero)
            rt.pivot = pivot;
    }

    // ─── Integration Scene ────────────────────────────────────────────────────

    private static void CreateIntegrationScene()
    {
        // Copy game scene → Minigame_FlappyVocab
        bool copied = false;
        if (AssetDatabase.LoadAssetAtPath<Object>(IntegScenePath) == null)
        {
            AssetDatabase.CopyAsset(GameScenePath, IntegScenePath);
            AssetDatabase.Refresh();
            copied = true;
        }

        // Open integration scene and add adapter
        var scene = EditorSceneManager.OpenScene(IntegScenePath, OpenSceneMode.Single);

        if (Object.FindFirstObjectByType<FPTSim.Minigames.FlappyVocabMinigame>() == null)
        {
            var adapterGo = new GameObject("[FlappyVocabAdapter]");
            var adapter   = adapterGo.AddComponent<FPTSim.Minigames.FlappyVocabMinigame>();

            // Auto-assign gameManager
            var mgr = Object.FindFirstObjectByType<FlappyVocabGame.FlappyVocabGameManager>();
            if (mgr != null)
            {
                SerializedObject so = new SerializedObject(adapter);
                so.FindProperty("gameManager").objectReferenceValue = mgr;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            Debug.Log("[FlappyVocabSetup] Tạo [FlappyVocabAdapter] trong Minigame_FlappyVocab");
        }

        EditorSceneManager.SaveScene(scene, IntegScenePath);
        AddToBuildSettings(IntegScenePath);
        Debug.Log($"[FlappyVocabSetup] Integration scene → {IntegScenePath}");
    }

    private static void AddToBuildSettings(string scenePath)
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list)
        {
            if (s.path == scenePath)
            {
                Debug.Log($"[FlappyVocabSetup] Build Settings: '{scenePath}' đã có.");
                return;
            }
        }
        list.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[FlappyVocabSetup] Build Settings: +{scenePath}");
    }

    // ─── Update Sprites (chạy sau khi đã có assets) ───────────────────────────

    [MenuItem("FPTSim/Update FlappyVocab Sprites")]
    public static void UpdateSprites()
    {
        ForceImportSpritesAsSprite();
        ApplySpritesToScene(GameScenePath);
        ApplySpritesToScene(IntegScenePath);
        Debug.Log("[FlappyVocabSetup] ✅ Sprites đã được cập nhật vào cả hai scene!");
    }

    /// <summary>
    /// Ép import tất cả PNG trong folder Sprites thành Sprite type.
    /// </summary>
    private static void ForceImportSpritesAsSprite()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D",
            new[] { "Assets/MiniGames/FlappyVocabGame/Sprites" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null && ti.textureType != TextureImporterType.Sprite)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                EditorUtility.SetDirty(ti);
                ti.SaveAndReimport();
            }
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Mở scene, tìm Bird và Background, gán đúng sprites + animation frames.
    /// </summary>
    private static void ApplySpritesToScene(string scenePath)
    {
        if (AssetDatabase.LoadAssetAtPath<Object>(scenePath) == null)
        {
            Debug.LogWarning($"[FlappyVocabSetup] Scene không tồn tại: {scenePath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // ── Background sprite ──
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BgSpritePath);
        if (bgSprite == null)
            Debug.LogWarning($"[FlappyVocabSetup] Không load được Background sprite: {BgSpritePath}");

        var bgGo = GameObject.Find("Background");
        if (bgGo != null && bgSprite != null)
        {
            var sr = bgGo.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite     = bgSprite;
                sr.color      = Color.white;
                sr.drawMode   = SpriteDrawMode.Sliced;
                sr.size       = new Vector2(20f, 12f);
                sr.sortingOrder = -10;
                EditorUtility.SetDirty(sr);
            }
        }

        // ── Bird sprite frames ──
        var birdGo = GameObject.Find("Bird");
        if (birdGo != null)
        {
            // Load all 7 frames
            Sprite[] frames = new Sprite[BirdFramePaths.Length];
            bool anyLoaded = false;
            for (int i = 0; i < BirdFramePaths.Length; i++)
            {
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(BirdFramePaths[i]);
                if (frames[i] != null) anyLoaded = true;
            }

            if (anyLoaded)
            {
                // Set initial sprite on SpriteRenderer
                var sr = birdGo.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = frames[0];
                    sr.color  = Color.white;
                    EditorUtility.SetDirty(sr);
                }

                // Assign frames to BirdController
                var ctrl = birdGo.GetComponent<FlappyVocabGame.BirdController>();
                if (ctrl != null)
                {
                    SerializedObject so = new SerializedObject(ctrl);
                    SerializedProperty framesProp = so.FindProperty("flyFrames");
                    framesProp.arraySize = frames.Length;
                    for (int i = 0; i < frames.Length; i++)
                        framesProp.GetArrayElementAtIndex(i).objectReferenceValue = frames[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                }

                // Set bird scale to match sprite aspect ratio
                birdGo.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
                EditorUtility.SetDirty(birdGo);
            }
        }

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[FlappyVocabSetup] Sprites applied → {scenePath}");
    }
}
