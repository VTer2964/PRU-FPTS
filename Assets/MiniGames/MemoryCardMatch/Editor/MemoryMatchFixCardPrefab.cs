using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using MemoryCardMatch;

namespace MemoryCardMatch.Editor
{
    public static class MemoryMatchFixCardPrefab
    {
        [MenuItem("Tools/MemoryCardMatch/Fix Card Prefab & Match Effect", false, 2)]
        public static void FixCardPrefab()
        {
            string prefabPath = "Assets/MemoryCardMatch/Prefabs/Card.prefab";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset == null) { Debug.LogError("[Fix] Card.prefab not found at " + prefabPath); return; }

            // Edit the prefab in isolation
            using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
            {
                GameObject root = scope.prefabContentsRoot;

                // ── 1. Root Image: transparent background (just for raycast)
                Image rootImg = root.GetComponent<Image>();
                if (rootImg != null)
                {
                    rootImg.color = Color.clear;
                    rootImg.raycastTarget = true;
                }

                // ── 2. Find CardBack, CardFront, MatchEffect children
                Transform cardBackT  = root.transform.Find("CardBack");
                Transform cardFrontT = root.transform.Find("CardFront");
                Transform matchEffT  = root.transform.Find("MatchEffect");

                // ── 3. Replace MatchEffect with a thin green border (not a full overlay)
                //      We do this by making MatchEffect use a border sprite approach:
                //      - Set it to a small-padded inset that is just a color outline
                //      - Crucially it must NOT fully cover CardFront
                if (matchEffT != null)
                {
                    RectTransform mRT = matchEffT.GetComponent<RectTransform>();
                    // Border sits *behind* the front image at the same size
                    mRT.anchorMin = Vector2.zero;
                    mRT.anchorMax = Vector2.one;
                    mRT.offsetMin = Vector2.zero;
                    mRT.offsetMax = Vector2.zero;

                    Image mImg = matchEffT.GetComponent<Image>();
                    if (mImg != null)
                    {
                        // Semi-transparent green tint — very low alpha so sprite shows through
                        mImg.color = new Color(0.25f, 0.95f, 0.40f, 0.25f);
                        mImg.raycastTarget = false;
                    }

                    // Move MatchEffect to be BEHIND CardFront (lower sibling index)
                    matchEffT.SetSiblingIndex(cardFrontT != null ? cardFrontT.GetSiblingIndex() : 1);
                    matchEffT.gameObject.SetActive(false);
                }

                // ── 4. Ensure CardFront image has preserveAspect = true (prevents sprite stretch)
                if (cardFrontT != null)
                {
                    Image cfImg = cardFrontT.GetComponent<Image>();
                    if (cfImg != null)
                    {
                        cfImg.preserveAspect = true;
                        cfImg.color = Color.white;
                    }

                    // Padding inset so the sprite doesn't touch card edges
                    RectTransform cfRT = cardFrontT.GetComponent<RectTransform>();
                    cfRT.anchorMin = Vector2.zero;
                    cfRT.anchorMax = Vector2.one;
                    cfRT.offsetMin = new Vector2(8, 8);
                    cfRT.offsetMax = new Vector2(-8, -8);
                }

                // ── 5. Ensure CardBack also preserveAspect
                if (cardBackT != null)
                {
                    Image cbImg = cardBackT.GetComponent<Image>();
                    if (cbImg != null) cbImg.preserveAspect = true;

                    RectTransform cbRT = cardBackT.GetComponent<RectTransform>();
                    cbRT.anchorMin = Vector2.zero;
                    cbRT.anchorMax = Vector2.one;
                    cbRT.offsetMin = Vector2.zero;
                    cbRT.offsetMax = Vector2.zero;
                }

                // ── 6. Wire CardController.cardBackgroundImage = MatchEffect Image
                //      (MatchEffect is now a soft overlay behind front image — acts as tinted bg)
                CardController cc = root.GetComponent<CardController>();
                if (cc != null && matchEffT != null)
                {
                    SerializedObject soCc = new SerializedObject(cc);
                    soCc.FindProperty("cardBackgroundImage").objectReferenceValue =
                        matchEffT.GetComponent<Image>();
                    soCc.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Fix] ✅ Card prefab updated — CardFront preserveAspect=true, MatchEffect is soft bg overlay.");
        }
    }
}
