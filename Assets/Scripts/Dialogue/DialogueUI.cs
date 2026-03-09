using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPTSim.Dialogue;
using FPTSim.Core;
using FPTSim.Audio;

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

        [Header("Audio (optional)")]
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip nextClip;
        [SerializeField] private AudioClip choiceClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip typingClip;

        [Header("Typewriter")]
        [SerializeField] private float charsPerSecond = 35f;
        [SerializeField] private float typingSoundInterval = 0.04f;

        private System.Action<int> onChoiceSelected;
        private System.Action onExit;

        private Coroutine typingRoutine;
        private DialogueNodeSO currentNode;
        private bool isTyping;
        private string fullText;
        private float typingSoundTimer;

        public bool IsTyping => isTyping;
        public bool IsOpen => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (exitButton)
            {
                exitButton.onClick.AddListener(() =>
                {
                    if (closeClip && AudioManager.I != null)
                        AudioManager.I.PlayDialogue(closeClip);

                    onExit?.Invoke();
                });
            }

            if (panel) panel.SetActive(false);
        }

        public void Open(System.Action<int> choiceCallback, System.Action exitCallback)
        {
            onChoiceSelected = choiceCallback;
            onExit = exitCallback;

            if (panel) panel.SetActive(true);

            if (crosshair) crosshair.SetActive(false);
            if (interactHintText) interactHintText.text = "";

            if (openClip && AudioManager.I != null)
                AudioManager.I.PlayDialogue(openClip);
        }

        public void Close()
        {
            StopTypingImmediate();

            if (panel) panel.SetActive(false);

            if (crosshair) crosshair.SetActive(true);

            ClearChoices();
            onChoiceSelected = null;
            onExit = null;
            currentNode = null;
        }

        public void RenderNode(DialogueNodeSO node)
        {
            if (node == null) return;

            currentNode = node;

            if (speakerText) speakerText.text = node.speakerName;

            ClearChoices();

            fullText = node.text ?? "";

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            typingRoutine = StartCoroutine(TypeTextRoutine(fullText));
        }

        public void CompleteTyping()
        {
            if (!isTyping) return;

            if (typingRoutine != null)
                StopCoroutine(typingRoutine);

            isTyping = false;

            if (bodyText) bodyText.text = fullText;

            ShowChoicesIfNeeded();
        }

        public void SetBodyText(string msg)
        {
            StopTypingImmediate();

            fullText = msg ?? "";
            if (bodyText) bodyText.text = fullText;

            ShowChoicesIfNeeded();
        }

        private IEnumerator TypeTextRoutine(string text)
        {
            isTyping = true;
            typingSoundTimer = 0f;

            if (bodyText) bodyText.text = "";

            float delay = (charsPerSecond <= 0f) ? 0.01f : 1f / charsPerSecond;

            for (int i = 0; i < text.Length; i++)
            {
                if (bodyText) bodyText.text += text[i];

                if (!char.IsWhiteSpace(text[i]) && typingClip && AudioManager.I != null)
                {
                    if (typingSoundTimer <= 0f)
                    {
                        AudioManager.I.PlayDialogue(typingClip, 0.35f);
                        typingSoundTimer = typingSoundInterval;
                    }
                }

                if (typingSoundTimer > 0f)
                    typingSoundTimer -= delay;

                yield return new WaitForSecondsRealtime(delay);
            }

            isTyping = false;
            ShowChoicesIfNeeded();
        }

        private void ShowChoicesIfNeeded()
        {
            if (currentNode == null) return;
            if (currentNode.choices == null || currentNode.choices.Length == 0) return;

            for (int i = 0; i < currentNode.choices.Length; i++)
            {
                var ch = currentNode.choices[i];

                // condition gate theo flag
                if (ch.requireCondition)
                {
                    string flag = ch.conditionKey != null ? ch.conditionKey.Trim() : "";
                    bool ok = GameManager.I != null &&
                              !string.IsNullOrWhiteSpace(flag) &&
                              GameManager.I.HasFlag(flag);

                    if (!ok) continue;
                }

                int realIndex = i;

                var btn = Instantiate(choiceButtonPrefab, choicesRoot);
                var tmp = btn.GetComponentInChildren<TMP_Text>();
                if (tmp) tmp.text = ch.text;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (choiceClip && AudioManager.I != null)
                        AudioManager.I.PlayDialogue(choiceClip);

                    onChoiceSelected?.Invoke(realIndex);
                });
            }
        }

        private void StopTypingImmediate()
        {
            if (typingRoutine != null)
            {
                StopCoroutine(typingRoutine);
                typingRoutine = null;
            }

            isTyping = false;
            typingSoundTimer = 0f;
        }

        private void ClearChoices()
        {
            if (!choicesRoot) return;

            for (int i = choicesRoot.childCount - 1; i >= 0; i--)
                Destroy(choicesRoot.GetChild(i).gameObject);
        }
    }
}