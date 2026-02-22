using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text endingText;
        [SerializeField] private Button restartButton;

        private void Start()
        {
            var gm = GameManager.I;
            if (gm == null) return;

            var code = gm.DecideEnding();
            endingText.text = code switch
            {
                "BAD_3_DISAPPEARED" => "BAD ENDING 3: Bạn biến mất bí ẩn...",
                "BAD_1_CHEAT" => "BAD ENDING 1: Bị phát hiện gian lận (Mr. Sus tool).",
                "BAD_2_STRESS" => "BAD ENDING 2: Áp lực quá lớn, bạn bỏ cuộc.",
                _ => "GOOD/NORMAL ENDING: Bạn hoàn thành hành trình 7 ngày!"
            };

            if (restartButton) restartButton.onClick.AddListener(Restart);
        }

        private void Restart()
        {
            GameManager.I.NewGame(deleteSave: true);
        }
    }
}