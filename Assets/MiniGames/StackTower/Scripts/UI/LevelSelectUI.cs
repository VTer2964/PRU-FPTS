using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace StackTower
{
    /// <summary>
    /// Displays the level selection grid.
    /// All levels are unlocked by default (as per spec).
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private Button backButton;

        [Header("Star Sprites (optional)")]
        [SerializeField] private Sprite starFilledSprite;
        [SerializeField] private Sprite starEmptySprite;

        // ── Private ──────────────────────────────────────────────────────────
        private readonly List<GameObject> _buttons = new List<GameObject>();

        // ── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Rebuild the level button grid.</summary>
        public void Refresh()
        {
            ClearButtons();

            var gm = StackTowerGameManager.Instance;
            if (gm == null) return;

            for (int i = 0; i < gm.LevelCount; i++)
            {
                LevelData ld = gm.GetLevelData(i);
                if (ld == null) continue;

                int savedStars = gm.GetSavedStars(i);
                CreateLevelButton(i, ld, savedStars);
            }
        }

        // ── Private ──────────────────────────────────────────────────────────

        private void CreateLevelButton(int index, LevelData ld, int savedStars)
        {
            GameObject btn = levelButtonPrefab != null
                ? Instantiate(levelButtonPrefab, levelButtonContainer)
                : CreateDefaultButton(index);

            _buttons.Add(btn);

            // Level number label
            var label = btn.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
                label.text = $"Level {ld.levelNumber}\n{ld.targetFloors} Tầng";

            // Stars display
            var stars = btn.GetComponentsInChildren<Image>();
            // Assumes button prefab has 3 Image children for stars (after first background)
            for (int s = 0; s < stars.Length && s < 3; s++)
            {
                if (starFilledSprite != null)
                    stars[s].sprite = s < savedStars ? starFilledSprite : starEmptySprite;
            }

            // Capture index for closure
            int levelIdx = index;
            var clickable = btn.GetComponent<Button>();
            if (clickable != null)
                clickable.onClick.AddListener(() => OnLevelClicked(levelIdx));
        }

        private GameObject CreateDefaultButton(int index)
        {
            // Fallback: create a plain UI button if no prefab is assigned
            GameObject go = new GameObject($"LevelBtn_{index + 1}");
            go.transform.SetParent(levelButtonContainer, false);

            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(160f, 120f);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f);

            var btn = go.AddComponent<Button>();

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 24;
            tmp.color = Color.white;

            return go;
        }

        private void ClearButtons()
        {
            foreach (var b in _buttons)
                if (b != null) Destroy(b);
            _buttons.Clear();
        }

        private void OnLevelClicked(int index)
        {
            StackTowerGameManager.Instance?.StartGame(index);
        }

        private void OnBackClicked()
        {
            gameObject.SetActive(false);
            StackTowerGameManager.Instance?.GoToMainMenu();
        }
    }
}
