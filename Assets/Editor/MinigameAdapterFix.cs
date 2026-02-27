using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Fix: chuyển adapter ra GO riêng [XxxAdapter], xóa khỏi GameManager.
/// </summary>
public static class MinigameAdapterFix
{
    [MenuItem("FPTSim/Fix Adapter Placement")]
    public static void FixAll()
    {
        FixCaro();
        FixMemory();
        Debug.Log("[MinigameAdapterFix] ✅ Xong!");
    }

    [MenuItem("FPTSim/Fix Adapter - Caro")]
    public static void FixCaro()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Minigame_Caro.unity", OpenSceneMode.Single);

        // 1. Xóa tất cả CaroMinigame hiện có (kể cả trên GameManager)
        foreach (var existing in Object.FindObjectsByType<FPTSim.Minigames.CaroMinigame>(FindObjectsSortMode.None))
            Object.DestroyImmediate(existing);

        // 2. Tạo GO [CaroAdapter] sạch
        var adapterGO = new GameObject("[CaroAdapter]");

        // 3. Gắn adapter
        var adapter = adapterGO.AddComponent<FPTSim.Minigames.CaroMinigame>();

        // 4. Assign caroGameManager reference
        var caroGM = Object.FindFirstObjectByType<CaroGame.CaroGameManager>();
        if (caroGM != null)
        {
            var so = new SerializedObject(adapter);
            so.FindProperty("caroGameManager").objectReferenceValue = caroGM;
            so.ApplyModifiedProperties();
            Debug.Log($"[MinigameAdapterFix] CaroMinigame → [CaroAdapter], caroGameManager = {caroGM.name}");
        }
        else
        {
            Debug.LogWarning("[MinigameAdapterFix] Không tìm thấy CaroGameManager trong scene!");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MinigameAdapterFix] Saved Minigame_Caro.unity");
    }

    [MenuItem("FPTSim/Fix Adapter - Memory")]
    public static void FixMemory()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Minigame_Memory.unity", OpenSceneMode.Single);

        // 1. Xóa tất cả MemoryMinigame hiện có
        foreach (var existing in Object.FindObjectsByType<FPTSim.Minigames.MemoryMinigame>(FindObjectsSortMode.None))
            Object.DestroyImmediate(existing);

        // 2. Tạo GO [MemoryAdapter] sạch
        var adapterGO = new GameObject("[MemoryAdapter]");

        // 3. Gắn adapter
        var adapter = adapterGO.AddComponent<FPTSim.Minigames.MemoryMinigame>();

        // 4. Assign memoryManager reference
        var memGM = Object.FindFirstObjectByType<MemoryCardMatch.MemoryMatchGameManager>();
        if (memGM != null)
        {
            var so = new SerializedObject(adapter);
            so.FindProperty("memoryManager").objectReferenceValue = memGM;
            so.ApplyModifiedProperties();
            Debug.Log($"[MinigameAdapterFix] MemoryMinigame → [MemoryAdapter], memoryManager = {memGM.name}");
        }
        else
        {
            Debug.LogWarning("[MinigameAdapterFix] Không tìm thấy MemoryMatchGameManager trong scene!");
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log("[MinigameAdapterFix] Saved Minigame_Memory.unity");
    }
}
