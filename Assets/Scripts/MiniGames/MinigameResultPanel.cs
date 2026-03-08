using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    public class MinigameResultPanel : MonoBehaviour
    {
        private const float AUTO_CLOSE_SECONDS = 5f;
        private const float ANIM_DURATION      = 0.45f;

        private System.Action onClosed;
        private float timer;
        private bool closing;
        private TMP_Text autoCloseText;
        private Image progressBarFill;
        private Transform cardT;

        // ── Factory ──────────────────────────────────────────────────────────

        public static MinigameResultPanel Show(MinigameResult result, System.Action onClosed)
        {
            var canvasGO = new GameObject("[MinigameResultPanel]");

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight  = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            var panel = canvasGO.AddComponent<MinigameResultPanel>();
            panel.onClosed = onClosed;
            panel.BuildUI(canvas, result);
            return panel;
        }

        // ── Build UI ─────────────────────────────────────────────────────────

        private void BuildUI(Canvas canvas, MinigameResult result)
        {
            Color accentColor, headerBg, glowColor;
            string medalLabel;
            switch (result.medal)
            {
                case Medal.Gold:
                    accentColor = new Color(1.00f, 0.82f, 0.10f);
                    headerBg    = new Color(0.55f, 0.38f, 0.02f, 1f);
                    glowColor   = new Color(1.00f, 0.90f, 0.30f, 0.35f);
                    medalLabel  = "HUY CHƯƠNG VÀNG";
                    break;
                case Medal.Silver:
                    accentColor = new Color(0.85f, 0.85f, 0.90f);
                    headerBg    = new Color(0.28f, 0.28f, 0.36f, 1f);
                    glowColor   = new Color(0.80f, 0.80f, 1.00f, 0.28f);
                    medalLabel  = "HUY CHƯƠNG BẠC";
                    break;
                case Medal.Bronze:
                    accentColor = new Color(0.85f, 0.52f, 0.18f);
                    headerBg    = new Color(0.38f, 0.20f, 0.04f, 1f);
                    glowColor   = new Color(1.00f, 0.60f, 0.20f, 0.28f);
                    medalLabel  = "HUY CHƯƠNG ĐỒNG";
                    break;
                default:
                    accentColor = new Color(0.55f, 0.55f, 0.60f);
                    headerBg    = new Color(0.20f, 0.10f, 0.10f, 1f);
                    glowColor   = new Color(0.60f, 0.20f, 0.20f, 0.20f);
                    medalLabel  = "KHÔNG ĐẠT";
                    break;
            }

            var ct = canvas.transform;

            // Dim overlay — full screen
            MakeImage(ct, "Overlay",
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.72f));

            // Soft glow circle
            MakeImage(ct, "Glow",
                Half, Half, new Vector2(580f, 580f), Vector2.zero,
                glowColor);

            // Card
            var cardGO = MakeImage(ct, "Card",
                Half, Half, new Vector2(460f, 430f), Vector2.zero,
                new Color(0.08f, 0.08f, 0.13f, 0.98f));
            cardT = cardGO.transform;

            // Header bar
            var headerGO = MakeImage(cardT, "Header",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 72f), new Vector2(0f, -36f),
                headerBg);
            var headerT = headerGO.transform;

            // "KẾT QUẢ" title
            var titleTxt = MakeTMP(headerT, "Title", "KẾT QUẢ", 23f, Color.white, FontStyles.Bold);
            titleTxt.alignment = TextAlignmentOptions.Center;
            Stretch(titleTxt.rectTransform, new Vector2(0f, 14f), new Vector2(0f, -6f));

            // Game id subtitle
            var subTxt = MakeTMP(headerT, "Sub", result.minigameId.ToUpper(),
                13f, new Color(1f, 1f, 1f, 0.50f), FontStyles.Normal);
            subTxt.alignment = TextAlignmentOptions.Center;
            var subRT = subTxt.rectTransform;
            SetAnchor(subRT, new Vector2(0f, 0f), new Vector2(1f, 0f));
            subRT.pivot            = new Vector2(0.5f, 1f);
            subRT.sizeDelta        = new Vector2(0f, 22f);
            subRT.anchoredPosition = new Vector2(0f, -2f);

            // Accent border bottom of header
            MakeImage(cardT, "Border",
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0f, 3f), new Vector2(0f, -72f),
                accentColor);

            // Medal badge — outer accent border
            var badgeOuter = MakeImage(cardT, "BadgeOuter",
                Half, Half, new Vector2(90f, 90f), new Vector2(0f, 68f), accentColor);

            // Badge inner — dark fill
            var badgeInner = MakeImage(badgeOuter.transform, "BadgeInner",
                Half, Half, new Vector2(74f, 74f), Vector2.zero,
                new Color(0.08f, 0.08f, 0.13f, 0.98f));

            // Badge tier text (I / II / III / X)
            string badgeTier = result.medal == Medal.Gold   ? "I"
                             : result.medal == Medal.Silver ? "II"
                             : result.medal == Medal.Bronze ? "III"
                             : "X";
            var badgeTxt = MakeTMP(badgeInner.transform, "Tier", badgeTier, 34f, accentColor, FontStyles.Bold);
            badgeTxt.alignment = TextAlignmentOptions.Center;
            StretchFull(badgeTxt.rectTransform);

            // Medal label
            var mlTxt = MakeTMP(cardT, "MedalLabel", medalLabel, 26f, accentColor, FontStyles.Bold);
            mlTxt.alignment = TextAlignmentOptions.Center;
            SetCenter(mlTxt.rectTransform, new Vector2(400f, 38f), new Vector2(0f, -4f));

            // Tier dots (3 pips — filled/dim by medal rank)
            int litDots = result.medal == Medal.Gold ? 3
                        : result.medal == Medal.Silver ? 2
                        : result.medal == Medal.Bronze ? 1
                        : 0;
            Color dimDot = new Color(accentColor.r, accentColor.g, accentColor.b, 0.18f);
            float[] dotXs = { -20f, 0f, 20f };
            for (int i = 0; i < 3; i++)
                MakeImage(cardT, $"Dot{i}", Half, Half,
                    new Vector2(10f, 10f), new Vector2(dotXs[i], -38f),
                    i < litDots ? accentColor : dimDot);

            // Separator
            MakeImage(cardT, "Sep",
                new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.5f),
                new Vector2(0f, 2f), new Vector2(0f, -58f),
                new Color(accentColor.r, accentColor.g, accentColor.b, 0.35f));

            // Progress bar background
            var barBgGO = MakeImage(cardT, "BarBg",
                Half, Half, new Vector2(360f, 8f), new Vector2(0f, -96f),
                new Color(0.18f, 0.18f, 0.22f));

            // Progress bar fill
            var fillGO = new GameObject("Fill");
            fillGO.transform.SetParent(barBgGO.transform, false);
            var fillRT = fillGO.AddComponent<RectTransform>();
            fillRT.anchorMin        = new Vector2(0f, 0f);
            fillRT.anchorMax        = new Vector2(1f, 1f);
            fillRT.pivot            = new Vector2(0f, 0.5f);
            fillRT.sizeDelta        = Vector2.zero;
            fillRT.anchoredPosition = Vector2.zero;
            progressBarFill            = fillGO.AddComponent<Image>();
            progressBarFill.color      = accentColor;
            progressBarFill.type       = Image.Type.Filled;
            progressBarFill.fillMethod = Image.FillMethod.Horizontal;
            progressBarFill.fillAmount = 1f;

            // Auto-close countdown text
            autoCloseText = MakeTMP(cardT, "AutoClose", "", 13f,
                new Color(0.45f, 0.45f, 0.50f), FontStyles.Normal);
            autoCloseText.alignment = TextAlignmentOptions.Center;
            SetCenter(autoCloseText.rectTransform, new Vector2(360f, 20f), new Vector2(0f, -114f));

            // Button — dùng sprite từ asset pack nếu load được, fallback plain color
            var btnGO = new GameObject("ContinueBtn");
            btnGO.transform.SetParent(cardT, false);
            var btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.anchorMin        = btnRT.anchorMax = Half;
            btnRT.pivot            = Half;
            btnRT.sizeDelta        = new Vector2(260f, 52f);
            btnRT.anchoredPosition = new Vector2(0f, -158f);

            var btnImg = btnGO.AddComponent<Image>();
            var continueSprite = Resources.Load<Sprite>("MinigameAssets_ContinueBtn");

            if (continueSprite != null)
            {
                btnImg.sprite = continueSprite;
                btnImg.type   = Image.Type.Simple;
                btnImg.preserveAspect = true;
                btnImg.color  = Color.white;
            }
            else
            {
                btnImg.color = accentColor;
            }

            var btn = btnGO.AddComponent<Button>();
            var cb  = btn.colors;
            cb.normalColor      = Color.white;
            cb.highlightedColor = new Color(0.80f, 0.80f, 0.80f);
            cb.pressedColor     = new Color(0.60f, 0.60f, 0.60f);
            cb.colorMultiplier  = 1f;
            btn.colors = cb;
            btn.onClick.AddListener(Close);

            // Nếu không có sprite → hiện text "TIẾP TỤC"
            if (continueSprite == null)
            {
                Color btnTextColor = result.medal == Medal.None
                    ? Color.white
                    : new Color(0.08f, 0.06f, 0.02f);
                var btnLbl = MakeTMP(btnGO.transform, "Label", "TIẾP TỤC", 20f, btnTextColor, FontStyles.Bold);
                btnLbl.alignment = TextAlignmentOptions.Center;
                StretchFull(btnLbl.rectTransform);
            }

            // Animate card in
            StartCoroutine(AnimateIn(cardGO.GetComponent<RectTransform>()));
        }

        // ── Animation ────────────────────────────────────────────────────────

        private IEnumerator AnimateIn(RectTransform rt)
        {
            rt.localScale = Vector3.zero;
            float elapsed = 0f;
            while (elapsed < ANIM_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ANIM_DURATION;
                float scale = t < 0.7f
                    ? Mathf.Lerp(0f, 1.08f, t / 0.7f)
                    : Mathf.Lerp(1.08f, 1.00f, (t - 0.7f) / 0.3f);
                rt.localScale = Vector3.one * scale;
                yield return null;
            }
            rt.localScale = Vector3.one;
        }

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void Update()
        {
            if (closing) return;

            timer += Time.deltaTime;
            float remaining = AUTO_CLOSE_SECONDS - timer;

            if (progressBarFill != null)
                progressBarFill.fillAmount = Mathf.Clamp01(remaining / AUTO_CLOSE_SECONDS);

            if (autoCloseText != null)
                autoCloseText.text = $"Tự động đóng sau {Mathf.Max(0, Mathf.CeilToInt(remaining))}s";

            if (timer >= AUTO_CLOSE_SECONDS)
                Close();
        }

        private void Close()
        {
            if (closing) return;
            closing = true;
            onClosed?.Invoke();
            Destroy(gameObject);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static readonly Vector2 Half = new Vector2(0.5f, 0.5f);

        private static GameObject MakeImage(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 pos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = anchorMin;
            rt.anchorMax        = anchorMax;
            rt.pivot            = Half;
            rt.sizeDelta        = sizeDelta;
            rt.anchoredPosition = pos;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private static TextMeshProUGUI MakeTMP(Transform parent, string name,
            string text, float size, Color color, FontStyles style)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text      = text;
            t.fontSize  = size;
            t.color     = color;
            t.fontStyle = style;
            t.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            return t;
        }

        private static void SetCenter(RectTransform rt, Vector2 size, Vector2 pos)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.sizeDelta        = size;
            rt.anchoredPosition = pos;
        }

        private static void SetAnchor(RectTransform rt, Vector2 min, Vector2 max)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
        }

        private static void Stretch(RectTransform rt, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.offsetMin        = offsetMin;
            rt.offsetMax        = offsetMax;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin        = Vector2.zero;
            rt.anchorMax        = Vector2.one;
            rt.sizeDelta        = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
