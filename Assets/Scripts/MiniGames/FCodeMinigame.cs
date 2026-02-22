using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    public class FCodeMinigame : MinigameBase
    {
        [Header("UI")]
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Button giveUpButton;

        [Header("Typing")]
        [SerializeField] private int length = 12;

        private string target;

        protected override void Start()
        {
            base.Start();
            minigameId = "FCode";
            target = GenerateTarget(length);

            if (promptText) promptText.text = target;
            if (hintText) hintText.text = "Gõ đúng chuỗi rồi nhấn Enter.";
            if (inputField)
            {
                inputField.text = "";
                inputField.ActivateInputField();
                inputField.onSubmit.AddListener(OnSubmit);
            }

            if (giveUpButton) giveUpButton.onClick.AddListener(GiveUp);
        }

        protected override void Update()
        {
            base.Update();
            if (timerText) timerText.text = $"Time: {timeLeft:0.0}s";
        }

        private void OnDestroy()
        {
            if (inputField) inputField.onSubmit.RemoveListener(OnSubmit);
            if (giveUpButton) giveUpButton.onClick.RemoveListener(GiveUp);
        }

        private void OnSubmit(string typed)
        {
            if (finished) return;

            typed = typed ?? "";
            int correct = CountCorrectPrefix(target, typed);
            float accuracy = target.Length == 0 ? 0f : (float)correct / target.Length;

            Medal medal;
            if (accuracy >= 0.9f) medal = Medal.Gold;
            else if (accuracy >= 0.7f) medal = Medal.Silver;
            else if (accuracy >= 0.5f) medal = Medal.Bronze;
            else medal = Medal.None;

            int score = GameManager.I != null ? GameManager.I.PointsFor(medal) : 0;

            Finish(new MinigameResult
            {
                minigameId = minigameId,
                medal = medal,
                scoreAwarded = score,
                success = medal != Medal.None
            });
        }

        private void GiveUp()
        {
            if (finished) return;
            Finish(new MinigameResult
            {
                minigameId = minigameId,
                medal = Medal.None,
                scoreAwarded = 0,
                success = false
            });
        }

        private static int CountCorrectPrefix(string a, string b)
        {
            int n = Mathf.Min(a.Length, b.Length);
            int c = 0;
            for (int i = 0; i < n; i++)
                if (a[i] == b[i]) c++;
                else break;
            return c;
        }

        private static string GenerateTarget(int len)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var arr = new char[len];
            for (int i = 0; i < len; i++)
                arr[i] = chars[Random.Range(0, chars.Length)];
            return new string(arr);
        }
    }
}