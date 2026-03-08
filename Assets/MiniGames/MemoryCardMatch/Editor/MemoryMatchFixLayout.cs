using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryCardMatch.Editor
{
    public static class MemoryMatchFixLayout
    {
        [MenuItem("Tools/MemoryCardMatch/Fix Difficulty Buttons", false, 1)]
        public static void FixDifficultyButtons()
        {
            // FindObjectsOfTypeAll finds inactive objects too
            GameObject diffRow = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "DifficultyButtons" && go.scene.IsValid())
                { diffRow = go; break; }
            }
            if (diffRow == null) { Debug.LogError("[Fix] Cannot find DifficultyButtons"); return; }

            // 1. Fix container size — must have explicit height
            RectTransform diffRT = diffRow.GetComponent<RectTransform>();
            diffRT.sizeDelta = new Vector2(700, 100);
            diffRT.anchoredPosition = new Vector2(0, -20);

            // 2. Fix HorizontalLayoutGroup — do NOT control height
            HorizontalLayoutGroup hlg = diffRow.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.childControlWidth       = true;
                hlg.childControlHeight      = false;   // ← KEY: don't override height
                hlg.childForceExpandWidth   = true;
                hlg.childForceExpandHeight  = false;
                hlg.spacing                 = 20;
                hlg.padding                 = new RectOffset(0, 0, 0, 0);
                hlg.childAlignment          = TextAnchor.MiddleCenter;
            }

            // 3. Fix each button child — set explicit height
            foreach (RectTransform childRT in diffRow.transform)
            {
                // Set height explicitly (width is driven by HLG, height is free)
                childRT.sizeDelta = new Vector2(childRT.sizeDelta.x, 100);
                childRT.anchoredPosition = new Vector2(childRT.anchoredPosition.x, 0);

                // Also add LayoutElement with minHeight so HLG respects it
                LayoutElement le = childRT.GetComponent<LayoutElement>();
                if (le == null) le = childRT.gameObject.AddComponent<LayoutElement>();
                le.minHeight      = 100;
                le.preferredHeight = 100;
                le.flexibleHeight  = 0;

                Debug.Log($"[Fix] Fixed button '{childRT.name}' height=100");
            }

            // 4. Fix PlayButton and DifficultyLabel using FindObjectsOfTypeAll
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!go.scene.IsValid()) continue;
                if (go.name == "PlayButton")
                {
                    var rt = go.GetComponent<RectTransform>();
                    if (rt != null) { rt.anchoredPosition = new Vector2(0, -180); rt.sizeDelta = new Vector2(400, 100); }
                    EditorUtility.SetDirty(go);
                }
                if (go.name == "DifficultyLabel")
                {
                    var rt = go.GetComponent<RectTransform>();
                    if (rt != null) rt.anchoredPosition = new Vector2(0, 90);
                    EditorUtility.SetDirty(go);
                }
            }

            // Mark scene dirty and save
            EditorUtility.SetDirty(diffRow);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[Fix] ✅ Difficulty buttons fixed! Height = 100, HLG no longer controls height.");
        }
    }
}
