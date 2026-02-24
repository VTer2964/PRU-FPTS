using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Dialogue;

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

            // Ẩn panel lúc bắt đầu
            if (panel) panel.SetActive(false);
        }

        public void Open(System.Action<int> choiceCallback, System.Action exitCallback)
        {
            onChoiceSelected = choiceCallback;
            onExit = exitCallback;

            if (panel) panel.SetActive(true);

            // ✅ Ẩn crosshair + xóa hint khi vào dialogue
            if (crosshair) crosshair.SetActive(false);
            if (interactHintText) interactHintText.text = "";
        }

        public void Close()
        {
            if (panel) panel.SetActive(false);

            // ✅ Hiện lại crosshair khi thoát dialogue
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
                int index = i;

                var btn = Instantiate(choiceButtonPrefab, choicesRoot);

                var tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp) tmp.text = node.choices[i].text;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onChoiceSelected?.Invoke(index));
            }
        }

        private void ClearChoices()
        {
            if (!choicesRoot) return;

            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
        }

        public void SetBodyText(string msg)
        {
            if (bodyText) bodyText.text = msg;
        }
    }
}