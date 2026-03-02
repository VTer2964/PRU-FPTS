using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    /// <summary>
    /// Overlay panel hiển thị điểm + medal sau khi minigame kết thúc.
    /// Được tạo động bởi MinigameBase.Finish() — không cần prefab.
    /// Tự động đóng sau autoCloseSeconds giây hoặc khi nhấn nút.
    /// </summary>
    public class MinigameResultPanel : MonoBehaviour
    {
        private const float AUTO_CLOSE_SECONDS = 5f;

        private System.Action onClosed;
        private float timer;
        private bool closing;
        private TMP_Text autoCloseText;

        // ── Factory ──────────────────────────────────────────────────────────

        public static MinigameResultPanel Show(MinigameResult result, System.Action onClosed)
        {
            var canvasGO = new GameObject("[MinigameResultPanel]");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var panel = canvasGO.AddComponent<MinigameResultPanel>();
            panel.onClosed = onClosed;
            panel.BuildUI(canvas, result);
            return panel;
        }

        // ── Build UI ─────────────────────────────────────────────────────────

        private void BuildUI(Canvas canvas, MinigameResult result)
        {
            // Dim background
            var bg = CreateRect(canvas.transform, "BG", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);

            // Card
            var card = CreateRect(canvas.transform, "Card",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(520, 400), Vector2.zero);
            card.gameObject.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.13f, 0.97f);

            var cardT = card.transform;

            // Title
            AddText(cardT, "Title", "KẾT QUẢ", 44, Color.white,
                new Vector2(0, 155), new Vector2(480, 56), FontStyles.Bold);

            // Game name
            AddText(cardT, "GameName", result.minigameId, 22, new Color(0.65f, 0.65f, 0.7f),
                new Vector2(0, 105), new Vector2(480, 32));

            // Divider (thin Image)
            var div = CreateRect(cardT, "Divider",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(420f, 2f), new Vector2(0f, 72f));
            div.gameObject.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.35f);

            // Score
            string scoreLabel = result.scoreAwarded > 0
                ? $"Điểm: <b>{result.scoreAwarded}</b>"
                : "Điểm: —";
            AddText(cardT, "Score", scoreLabel, 34, new Color(1f, 0.9f, 0.3f),
                new Vector2(0, 20), new Vector2(480, 48));

            // Medal
            (string name, Color color) medalInfo = result.medal switch
            {
                Medal.Gold   => ("🥇  HUY CHƯƠNG VÀNG",  new Color(1.00f, 0.84f, 0.00f)),
                Medal.Silver => ("🥈  HUY CHƯƠNG BẠC",   new Color(0.80f, 0.80f, 0.80f)),
                Medal.Bronze => ("🥉  HUY CHƯƠNG ĐỒNG",  new Color(0.80f, 0.50f, 0.20f)),
                _            => ("✖  KHÔNG ĐẠT",         new Color(0.70f, 0.25f, 0.25f)),
            };
            AddText(cardT, "Medal", medalInfo.name, 28, medalInfo.color,
                new Vector2(0, -38), new Vector2(480, 42), FontStyles.Bold);

            // Auto-close countdown
            autoCloseText = AddText(cardT, "AutoClose", "", 17, new Color(0.5f, 0.5f, 0.55f),
                new Vector2(0, -102), new Vector2(480, 28));

            // Continue button
            AddButton(cardT);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private RectTransform CreateRect(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        private TMP_Text AddText(Transform parent, string name, string content,
            float fontSize, Color color, Vector2 pos, Vector2 size,
            FontStyles style = FontStyles.Normal)
        {
            var rt = CreateRect(parent, name,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, pos);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = content;
            t.fontSize = fontSize;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.fontStyle = style;
            t.enableWordWrapping = false;
            return t;
        }

        private void AddButton(Transform parent)
        {
            var rt = CreateRect(parent, "ContinueBtn",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(240f, 58f), new Vector2(0f, -162f));
            rt.gameObject.AddComponent<Image>().color = new Color(0.15f, 0.55f, 0.25f);
            var btn = rt.gameObject.AddComponent<Button>();

            var colors = btn.colors;
            colors.highlightedColor = new Color(0.25f, 0.75f, 0.35f);
            colors.pressedColor = new Color(0.10f, 0.40f, 0.18f);
            btn.colors = colors;

            btn.onClick.AddListener(Close);

            // Label
            var labelRT = CreateRect(rt, "Label", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var label = labelRT.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = "Tiếp tục";
            label.fontSize = 24;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
        }

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Update()
        {
            if (closing) return;

            timer += Time.deltaTime;
            int remaining = Mathf.CeilToInt(AUTO_CLOSE_SECONDS - timer);

            if (autoCloseText != null)
                autoCloseText.text = $"Tự động đóng sau {Mathf.Max(remaining, 0)}s";

            if (timer >= AUTO_CLOSE_SECONDS)
                Close();
        }

        private void Close()
        {
            if (closing) return;
            closing = true;
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            yield return null; // chờ 1 frame tránh double-invoke
            onClosed?.Invoke();
            Destroy(gameObject);
        }
    }
}
