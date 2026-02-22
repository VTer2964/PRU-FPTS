using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using FPTSim.Minigames;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class MinigameButtonItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descText;
        [SerializeField] private Button playButton;

        private MinigameInfoSO info;

        public void Bind(MinigameInfoSO minigame, bool canPlay)
        {
            info = minigame;

            if (titleText) titleText.text = minigame.displayName;
            if (descText) descText.text = minigame.description;
            if (playButton)
            {
                playButton.interactable = canPlay;
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(Play);
            }
        }

        private void Play()
        {
            if (GameManager.I == null) return;
            if (!GameManager.I.CanPlayMinigame()) return;

            // load minigame scene
            SceneManager.LoadScene(info.sceneName);
        }
    }
}