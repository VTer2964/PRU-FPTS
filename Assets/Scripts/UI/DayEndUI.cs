using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class DayEndUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text summaryText;
        [SerializeField] private Button nextDayButton;

        private void Start()
        {
            Refresh();
            if (nextDayButton) nextDayButton.onClick.AddListener(Next);
        }

        private void Refresh()
        {
            var gm = GameManager.I;
            if (gm == null) return;

            var s = gm.State;
            var c = gm.Config;

            summaryText.text =
                $"END OF DAY {s.currentDay}\n" +
                $"Played: {s.playedMinigamesToday}/{c.maxMinigamesPerDay}\n" +
                $"Score: {s.totalScore}\n" +
                $"Medals: G:{s.gold} S:{s.silver} B:{s.bronze}\n" +
                $"Stress: {s.stress}\n\n" +
                $"(Prototype cutscene text here)";
        }

        private void Next()
        {
            GameManager.I.NextDayOrFinish();
        }
    }
}