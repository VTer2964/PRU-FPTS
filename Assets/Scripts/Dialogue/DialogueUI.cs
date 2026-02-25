using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Dialogue;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class DialogueUI_Genshin : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject panel;

        [Header("Texts")]
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text bodyText;

        [Header("Choices")]
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button choiceButtonPrefab;

        [Header("Exit")]
        [SerializeField] private Button exitButton;

        [Header("HUD (Hide when dialogue open)")]
        [SerializeField] private GameObject crosshair;
        [SerializeField] private TMP_Text interactHintText;

        private System.Action<int> onChoiceSelected;
        private System.Action onExit;

        private void Awake()
        {
            if (exitButton) exitButton.onClick.AddListener(() => onExit?.Invoke());
            if (panel) panel.SetActive(false);
        }

        public void Open(System.Action<int> choiceCallback, System.Action exitCallback)
        {
            onChoiceSelected = choiceCallback;
            onExit = exitCallback;

            if (panel) panel.SetActive(true);

            if (crosshair) crosshair.SetActive(false);
            if (interactHintText) interactHintText.text = "";
        }

        public void Close()
        {
            if (panel) panel.SetActive(false);

            if (crosshair) crosshair.SetActive(true);

            ClearChoices();
            onChoiceSelected = null;
            onExit = null;
        }

        public void RenderNode(DialogueNodeSO node)
        {
            if (node == null) return;

            if (speakerText) speakerText.text = node.speakerName;
            if (bodyText) bodyText.text = node.text;

            ClearChoices();

            if (node.choices == null || node.choices.Length == 0)
                return;

            for (int i = 0; i < node.choices.Length; i++)
            {
                var ch = node.choices[i];

                // ✅ Condition gate by story flag
                if (ch.requireCondition)
                {
                    string flag = ch.conditionKey != null ? ch.conditionKey.Trim() : "";
                    bool ok = GameManager.I != null && !string.IsNullOrWhiteSpace(flag) && GameManager.I.HasFlag(flag);

                    if (!ok) continue; // ẩn choice này
                }

                int realIndex = i; // index thật của choice trong node.choices

                var btn = Instantiate(choiceButtonPrefab, choicesRoot);
                var tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp) tmp.text = ch.text;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onChoiceSelected?.Invoke(realIndex));
            }
        }

        public void SetBodyText(string msg)
        {
            if (bodyText) bodyText.text = msg;
        }

        private void ClearChoices()
        {
            if (!choicesRoot) return;
            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
        }
    }
}