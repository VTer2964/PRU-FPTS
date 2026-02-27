using UnityEngine;
using UnityEditor;
using FPTSim.Dialogue;

/// <summary>
/// Tạo DialogueNodeSO + DialogueGraphSO cho 4 minigame.
/// Menu: FPTSim/Create Minigame Dialogues
/// </summary>
public static class MinigameDialogueCreator
{
    private const string Folder = "Assets/Dialogue/Minigames";

    private static readonly (string id, string displayName, string sceneName)[] Minigames =
    {
        ("Caro",         "Caro",         "Minigame_Caro"),
        ("CountryGuess", "Country Guess", "Minigame_CountryGuess"),
        ("Memory",       "Memory Match",  "Minigame_Memory"),
        ("StackTower",   "Stack Tower",   "Minigame_StackTower"),
    };

    [MenuItem("FPTSim/Create Minigame Dialogues")]
    public static void CreateAll()
    {
        EnsureFolder("Assets/Dialogue");
        EnsureFolder(Folder);

        foreach (var (id, displayName, sceneName) in Minigames)
            CreateDialogueSet(id, displayName, sceneName);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[MinigameDialogueCreator] ✅ Tạo xong dialogue cho 4 minigame!");
    }

    private static void CreateDialogueSet(string id, string displayName, string sceneName)
    {
        // --- Node: Close (chọn Không) ---
        var nodeClose = GetOrCreate<DialogueNodeSO>($"{Folder}/Node_Close_{id}.asset");
        nodeClose.speakerType   = DialogueSpeakerType.NPC;
        nodeClose.speakerName   = displayName;
        nodeClose.text          = "Hẹn gặp lại nhé!";
        nodeClose.triggerAction = true;
        nodeClose.actionType    = DialogueActionType.CloseDialogue;
        nodeClose.choices       = null;
        EditorUtility.SetDirty(nodeClose);

        // --- Node: Action (chọn Có → load scene) ---
        var nodeAction = GetOrCreate<DialogueNodeSO>($"{Folder}/Node_Action_{id}.asset");
        nodeAction.speakerType   = DialogueSpeakerType.NPC;
        nodeAction.speakerName   = displayName;
        nodeAction.text          = "Bắt đầu thôi!";
        nodeAction.triggerAction = true;
        nodeAction.actionType    = DialogueActionType.StartMinigameScene;
        nodeAction.actionParam   = sceneName;
        nodeAction.choices       = null;
        EditorUtility.SetDirty(nodeAction);

        // --- Node: Text (entry — hỏi player) ---
        var nodeText = GetOrCreate<DialogueNodeSO>($"{Folder}/Node_Text_{id}.asset");
        nodeText.speakerType   = DialogueSpeakerType.NPC;
        nodeText.speakerName   = displayName;
        nodeText.text          = $"Bạn có muốn chơi {displayName} không?";
        nodeText.triggerAction = false;
        nodeText.autoAdvance   = false;
        nodeText.choices = new FPTSim.Dialogue.DialogueChoice[]
        {
            new FPTSim.Dialogue.DialogueChoice { text = "Có, chơi thôi!",  next = nodeAction },
            new FPTSim.Dialogue.DialogueChoice { text = "Không, để sau.",  next = nodeClose  },
        };
        EditorUtility.SetDirty(nodeText);

        // --- Graph ---
        var graph = GetOrCreate<DialogueGraphSO>($"{Folder}/Graph_{id}.asset");
        graph.graphId   = $"Minigame_{id}";
        graph.entryNode = nodeText;
        EditorUtility.SetDirty(graph);

        Debug.Log($"[MinigameDialogueCreator] ✅ {id}: Graph + 3 nodes tạo xong.");
    }

    private static T GetOrCreate<T>(string path) where T : ScriptableObject
    {
        var existing = AssetDatabase.LoadAssetAtPath<T>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
