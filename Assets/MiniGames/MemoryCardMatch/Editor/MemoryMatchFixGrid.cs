using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryCardMatch.Editor
{
    public static class MemoryMatchFixGrid
    {
        [MenuItem("Tools/MemoryCardMatch/Fix Grid Centering", false, 3)]
        public static void FixGridCentering()
        {
            // Find CardGrid (GridLayoutGroup) — may be inactive
            GridLayoutGroup glg = null;
            foreach (var go in Resources.FindObjectsOfTypeAll<GridLayoutGroup>())
            {
                if (go.gameObject.name == "CardGrid" && go.gameObject.scene.IsValid())
                { glg = go; break; }
            }

            if (glg == null) { Debug.LogError("[Fix] CardGrid not found."); return; }

            // 1. Center child alignment so cards sit in the middle of the grid area
            glg.childAlignment = TextAnchor.MiddleCenter;
            EditorUtility.SetDirty(glg.gameObject);

            // 2. Make sure CardGrid RectTransform is fully stretched inside its parent
            RectTransform gridRT = glg.GetComponent<RectTransform>();
            gridRT.anchorMin     = Vector2.zero;
            gridRT.anchorMax     = Vector2.one;
            gridRT.offsetMin     = Vector2.zero;
            gridRT.offsetMax     = Vector2.zero;
            EditorUtility.SetDirty(gridRT);

            // 3. Make sure CardGridContainer is also fully stretched
            Transform containerT = glg.transform.parent;
            if (containerT != null)
            {
                RectTransform contRT = containerT.GetComponent<RectTransform>();
                if (contRT != null)
                {
                    contRT.anchorMin  = new Vector2(0f, 0f);
                    contRT.anchorMax  = new Vector2(1f, 1f);
                    contRT.offsetMin  = new Vector2(20f, 80f);   // bottom gap for potential banner
                    contRT.offsetMax  = new Vector2(-20f, -120f); // top gap for HUD
                    EditorUtility.SetDirty(contRT);
                }
            }

            // 4. Save scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[Fix] ✅ Grid centering fixed — childAlignment=MiddleCenter, anchors stretched.");
        }
    }
}
