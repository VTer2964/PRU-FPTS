using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    public class SpaceNhanhMinigame : MinigameBase
    {
        [Header("UI")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text targetText;
        [SerializeField] private TMP_Text medalPreviewText;
        [SerializeField] private Button finishButton;

        [Header("Settings")]
        [SerializeField] private int goldTarget = 40;
        [SerializeField] private int silverTarget = 25;
        [SerializeField] private int bronzeTarget = 15;

        private int spaceCount;

        protected override void Start()
        {
            // set id + time limit trước khi base.Start()
            minigameId = "SpaceNhanh";
            timeLimit = 10f;

            base.Start();

            spaceCount = 0;

            if (targetText)
                targetText.text = $"Goal: Gold ≥ {goldTarget} | Silver ≥ {silverTarget} | Bronze ≥ {bronzeTarget}";

            if (finishButton)
                finishButton.onClick.AddListener(GiveUpOrFinish);

            UpdateUI();
        }

        protected override void Update()
        {
            if (!finished)
            {
                // Bắt phím Space
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    spaceCount++;
                    UpdateUI();
                }
            }

            base.Update(); // giảm timer, hết giờ thì gọi OnTimeUp()

            if (timerText)
                timerText.text = $"Time: {timeLeft:0.0}s";
        }

        protected override void OnTimeUp()
        {
            // Hết giờ => tính medal theo số lần bấm và kết thúc
            FinishWithResult();
        }

        private void GiveUpOrFinish()
        {
            if (finished) return;
            FinishWithResult();
        }

        private void FinishWithResult()
        {
            Medal medal = CalculateMedal(spaceCount);

            Finish(new MinigameResult
            {
                minigameId = minigameId,
                medal = medal,
                scoreAwarded = 0,  // bạn đã bỏ điểm, để 0
                success = medal != Medal.None
            });
        }

        private Medal CalculateMedal(int count)
        {
            if (count >= goldTarget) return Medal.Gold;
            if (count >= silverTarget) return Medal.Silver;
            if (count >= bronzeTarget) return Medal.Bronze;
            return Medal.None;
        }

        private void UpdateUI()
        {
            if (countText) countText.text = $"Space: {spaceCount}";

            Medal preview = CalculateMedal(spaceCount);
            if (medalPreviewText)
                medalPreviewText.text = $"Medal: {preview}";
        }

        private void OnDestroy()
        {
            if (finishButton)
                finishButton.onClick.RemoveListener(GiveUpOrFinish);
        }
    }
}