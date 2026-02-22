using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Core;
using FPTSim.Minigames;

namespace FPTSim.UI
{
    public class MinigameSelectUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private MinigameInfoSO[] minigames;

        [Header("UI")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private Button closeButton;

        [Header("Prefabs")]
        [SerializeField] private MinigameButtonItem itemPrefab;

        [SerializeField] private HUDController hudController;

        private void OnEnable()
        {
            BuildList();
            if (closeButton) closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            if (closeButton) closeButton.onClick.RemoveListener(Close);
        }

        private void Close()
        {
            gameObject.SetActive(false);

            if (hudController)
                hudController.CloseMinigamePanel();
        }

        private void BuildList()
        {
            if (contentRoot == null || itemPrefab == null) return;

            // clear
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);

            bool canPlay = GameManager.I != null && GameManager.I.CanPlayMinigame();

            foreach (var mg in minigames)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                item.Bind(mg, canPlay);
            }
        }
    }
}