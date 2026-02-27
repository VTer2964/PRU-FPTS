using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class MinigameSetup
{
    // ── Scene gốc (Hướng A) ───────────────────────────────────────────────────
    private const string CaroScenePath       = "Assets/MiniGames/CaroGame/Scenes/CaroGame.unity";
    private const string CountryScenePath    = "Assets/MiniGames/CountryGuessGame/Scenes/CountryGameScene.unity";
    private const string MemoryScenePath     = "Assets/MiniGames/MemoryCardMatch/Scenes/MemoryCardMatch.unity";
    private const string StackTowerScenePath = "Assets/MiniGames/StackTower/Scenes/StackTowerScene.unity";

    // ── Scene đích Minigame_ (đã copy sẵn bằng Bash) ─────────────────────────
    private const string CaroDst       = "Assets/Scenes/Minigame_Caro.unity";
    private const string CountryDst    = "Assets/Scenes/Minigame_CountryGuess.unity";
    private const string MemoryDst     = "Assets/Scenes/Minigame_Memory.unity";
    private const string StackDst      = "Assets/Scenes/Minigame_StackTower.unity";

    // ─────────────────────────────────────────────────────────────────────────
    //  Bước 2-3-4: Gắn adapter + Lưu + Build Settings (scene đã copy sẵn)
    // ─────────────────────────────────────────────────────────────────────────

    [MenuItem("FPTSim/Setup ALL Minigames")]
    public static void SetupAll()
    {
        AttachAndRegister<FPTSim.Minigames.CaroMinigame>      (CaroDst,    "CaroAdapter");
        AttachAndRegister<FPTSim.Minigames.CountryGuessMinigame>(CountryDst, "CountryGuessAdapter");
        AttachAndRegister<FPTSim.Minigames.MemoryMinigame>    (MemoryDst,  "MemoryAdapter");
        AttachAndRegister<FPTSim.Minigames.StackTowerMinigame>(StackDst,   "StackTowerAdapter");
        Debug.Log("[MinigameSetup] ✅✅✅ Tất cả minigame đã setup xong!");
    }

    [MenuItem("FPTSim/Setup Minigame Caro")]
    public static void SetupCaro()
        => AttachAndRegister<FPTSim.Minigames.CaroMinigame>(CaroDst, "CaroAdapter");

    [MenuItem("FPTSim/Setup Minigame CountryGuess")]
    public static void SetupCountryGuess()
        => AttachAndRegister<FPTSim.Minigames.CountryGuessMinigame>(CountryDst, "CountryGuessAdapter");

    [MenuItem("FPTSim/Setup Minigame Memory")]
    public static void SetupMemory()
        => AttachAndRegister<FPTSim.Minigames.MemoryMinigame>(MemoryDst, "MemoryAdapter");

    [MenuItem("FPTSim/Setup Minigame StackTower")]
    public static void SetupStackTower()
        => AttachAndRegister<FPTSim.Minigames.StackTowerMinigame>(StackDst, "StackTowerAdapter");

    // ─────────────────────────────────────────────────────────────────────────

    private static void AttachAndRegister<T>(string scenePath, string adapterGoName)
        where T : MonoBehaviour
    {
        if (!System.IO.File.Exists(scenePath.Replace("Assets/", "Assets/")))
        {
            // kiểm tra bằng AssetDatabase
        }
        if (AssetDatabase.LoadAssetAtPath<Object>(scenePath) == null)
        {
            Debug.LogError($"[MinigameSetup] ❌ Scene không tồn tại: {scenePath}. Hãy copy file trước.");
            return;
        }

        // Bước 2: Mở scene
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Tạo GO [XxxAdapter] nếu chưa có
        var existing = Object.FindFirstObjectByType<T>();
        if (existing == null)
        {
            var go = new GameObject($"[{adapterGoName}]");
            go.AddComponent<T>();
            Debug.Log($"[MinigameSetup] Tạo [{adapterGoName}] + {typeof(T).Name} trong {scenePath}");
        }
        else
        {
            Debug.Log($"[MinigameSetup] {typeof(T).Name} đã tồn tại trên '{existing.name}', bỏ qua.");
        }

        // Bước 3: Lưu scene
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[MinigameSetup] Saved: {scenePath}");

        // Bước 4: Thêm vào Build Settings
        AddToBuildSettings(scenePath);
    }

    private static void AddToBuildSettings(string scenePath)
    {
        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var s in list)
        {
            if (s.path == scenePath)
            {
                Debug.Log($"[MinigameSetup] Build Settings: '{scenePath}' đã có.");
                return;
            }
        }
        list.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[MinigameSetup] Build Settings: +{scenePath}");
    }
}
