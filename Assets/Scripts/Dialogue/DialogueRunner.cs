using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using FPTSim.Core;
using FPTSim.UI;

namespace FPTSim.Dialogue
{
    public class DialogueRunner : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private DialogueUI_Genshin ui;

        [Header("Camera (optional)")]
        [SerializeField] private DialogueCameraController camCtrl;

        [Header("Lock Player While Talking")]
        [SerializeField] private FPTSim.Player.MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;

        [Header("Input")]
        [SerializeField] private Key continueKey = Key.Space;
        [SerializeField] private Key exitKey = Key.Escape;
        [SerializeField] private SettingsUIController settingsUI;
        [SerializeField] private FPTSim.Minigames.CampusMinigameRenderHost minigameHost;
        [SerializeField] private float forcedAdvanceDelay = 0.45f;

        private DialogueGraphSO currentGraph;
        private DialogueNodeSO currentNode;
        private string runtimeMessage;
        private bool allowManualAdvance = true;
        private bool allowManualExit = true;
        private bool forceAutoAdvanceWithoutInput;
        private float forcedAdvanceTimer = -1f;

        public bool IsRunning => currentGraph != null;

        public System.Action OnDialogueStopped;

        public void StartDialogue(DialogueGraphSO graph)
        {
            StartDialogue(graph, true, true, false);
        }

        public void StartDialogue(DialogueGraphSO graph, bool allowManualAdvance, bool allowManualExit, bool forceAutoAdvanceWithoutInput)
        {
            if (graph == null || graph.entryNode == null) return;
            if (ui == null)
            {
                Debug.LogError("[DialogueRunner] Missing DialogueUI_Genshin reference!");
                return;
            }

            currentGraph = graph;
            currentNode = graph.entryNode;
            this.allowManualAdvance = allowManualAdvance;
            this.allowManualExit = allowManualExit;
            this.forceAutoAdvanceWithoutInput = forceAutoAdvanceWithoutInput;
            forcedAdvanceTimer = -1f;

            ui.Open(OnChoiceSelected, StopDialogue, allowManualExit);
            LockPlayer(true);

            ShowNode(currentNode);
        }

        private void Update()
        {
            if (!IsRunning) return;

            if (settingsUI == null)
                settingsUI = FindFirstObjectByType<SettingsUIController>();

            if (minigameHost == null)
                minigameHost = FindFirstObjectByType<FPTSim.Minigames.CampusMinigameRenderHost>();

            if (settingsUI != null && settingsUI.IsOpen)
                return;

            if (allowManualExit && Keyboard.current != null && Keyboard.current[exitKey].wasPressedThisFrame)
            {
                if (minigameHost != null && minigameHost.IsRunning)
                    return;

                // Reserve ESC for HUD/settings so dialogue can stay alive under the overlay.
                if (settingsUI != null)
                    return;

                StopDialogue();
                return;
            }

            if (currentNode == null) return;

            if (forceAutoAdvanceWithoutInput)
            {
                HandleForcedAdvance();
                return;
            }

            bool pressContinue =
                (Keyboard.current != null && Keyboard.current[continueKey].wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (!pressContinue) return;
            if (!allowManualAdvance) return;

            if (ui != null && ui.IsTyping)
            {
                ui.CompleteTyping();
                return;
            }

            bool hasChoices = currentNode.choices != null && currentNode.choices.Length > 0;
            if (hasChoices) return;
            if (!currentNode.autoAdvance) return;

            ContinueAuto();
        }

        private void ContinueAuto()
        {
            if (currentNode == null) return;

            if (currentNode.nextAuto == null)
            {
                StopDialogue();
                return;
            }

            currentNode = currentNode.nextAuto;
            ShowNode(currentNode);
        }

        private void HandleForcedAdvance()
        {
            if (ui != null && ui.IsTyping)
            {
                forcedAdvanceTimer = -1f;
                return;
            }

            bool hasChoices = currentNode.choices != null && currentNode.choices.Length > 0;
            if (hasChoices)
            {
                forcedAdvanceTimer = -1f;
                return;
            }

            if (forcedAdvanceTimer < 0f)
                forcedAdvanceTimer = Mathf.Max(0f, forcedAdvanceDelay);

            forcedAdvanceTimer -= Time.unscaledDeltaTime;
            if (forcedAdvanceTimer > 0f) return;

            if (currentNode.nextAuto == null)
            {
                StopDialogue();
                return;
            }

            currentNode = currentNode.nextAuto;
            ShowNode(currentNode);
        }

        private void ShowNode(DialogueNodeSO node)
        {
            if (node == null)
            {
                StopDialogue();
                return;
            }

            runtimeMessage = null;
            forcedAdvanceTimer = -1f;

            if (node.triggerAction)
            {
                ExecuteAction(node);
                if (currentGraph == null) return;
            }

            ui.RenderNode(node);

            if (!string.IsNullOrWhiteSpace(runtimeMessage))
                ui.SetBodyText(runtimeMessage);
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            if (currentNode == null || currentNode.choices == null) return;
            if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Length) return;

            var choice = currentNode.choices[choiceIndex];

            if (choice.next == null)
            {
                StopDialogue();
                return;
            }

            currentNode = choice.next;
            ShowNode(currentNode);
        }

        public void StopDialogue()
        {
            if (currentGraph == null) return;

            ui.Close();
            LockPlayer(false);

            camCtrl?.Clear();

            currentGraph = null;
            currentNode = null;
            allowManualAdvance = true;
            allowManualExit = true;
            forceAutoAdvanceWithoutInput = false;
            forcedAdvanceTimer = -1f;

            OnDialogueStopped?.Invoke();
        }

        private void LockPlayer(bool talking)
        {
            if (mouseLook) mouseLook.LockCursor(!talking);
            if (playerMovement) playerMovement.enabled = !talking;
            if (playerInteractor) playerInteractor.enabled = !talking;
        }

        private void ExecuteAction(DialogueNodeSO node)
        {
            var gm = GameManager.I;

            switch (node.actionType)
            {
                case DialogueActionType.SetCamera:
                    {
                        if (camCtrl == null)
                        {
                            runtimeMessage = "Loi: chua gan DialogueCameraController.";
                            break;
                        }

                        string key = node.actionParam != null ? node.actionParam.Trim() : "";
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            runtimeMessage = "Loi: SetCamera thieu actionParam (cameraKey).";
                            break;
                        }

                        bool ok = camCtrl.Focus(key);
                        if (!ok) runtimeMessage = $"Loi: khong tim thay camera key '{key}'.";
                        break;
                    }

                case DialogueActionType.StartMinigameScene:
                    {
                        string scene = node.actionParam != null ? node.actionParam.Trim() : "";
                        StopDialogue();

                        if (string.IsNullOrWhiteSpace(scene))
                        {
                            Debug.LogError("[DialogueRunner] StartMinigameScene: actionParam(scene name) is empty!");
                            return;
                        }
                        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                        if (playerObj == null)
                        {
                            Debug.LogError("[DialogueRunner] Khong tim thay Player co tag Player.");
                            return;
                        }

                        var host = FindFirstObjectByType<FPTSim.Minigames.CampusMinigameRenderHost>();
                        if (host == null)
                        {
                            Debug.LogError("[DialogueRunner] Khong tim thay CampusMinigameRenderHost trong scene Campus.");
                            return;
                        }

                        bool started = host.StartMinigame(scene, playerObj.transform);
                        if (!started)
                        {
                            Debug.LogWarning($"[DialogueRunner] Khong the start minigame scene '{scene}'.");
                        }

                        break;
                    }

                case DialogueActionType.BuyTimeWithBronze:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithBronze();
                        runtimeMessage = ok
                            ? "OK roi em nha"
                            : "Khong du huy chuong dong?";
                        break;
                    }

                case DialogueActionType.BuyTimeWithSilver:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithSilver();
                        runtimeMessage = ok
                            ? "OK roi em nha"
                            : "Khong du huy chuong bac";
                        break;
                    }

                case DialogueActionType.BuyTimeWithGold:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithGold();
                        runtimeMessage = ok
                            ? "OK roi em nha"
                            : "Khong du huy chuong vang";
                        break;
                    }

                case DialogueActionType.SubmitToWin:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        bool ok = gm.TrySubmitToWin();
                        if (!ok)
                        {
                            var c = gm.Config;
                            runtimeMessage =
                                $"Chua du huy chuong yeu cau!\n" +
                                $"Can: G{c.requiredGold}  S{c.requiredSilver}  B{c.requiredBronze}";
                        }
                        break;
                    }

                case DialogueActionType.SetFlag:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        string flag = node.actionParam != null ? node.actionParam.Trim() : "";
                        if (string.IsNullOrWhiteSpace(flag))
                        {
                            runtimeMessage = "Loi: SetFlag thieu actionParam (ten flag).";
                            break;
                        }

                        gm.SetFlag(flag);
                        break;
                    }

                case DialogueActionType.AddBronze:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        int amount = Mathf.Max(1, node.actionValue);
                        gm.AddMedal(Medal.Bronze, amount);
                        break;
                    }

                case DialogueActionType.AddSilver:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        int amount = Mathf.Max(1, node.actionValue);
                        gm.AddMedal(Medal.Silver, amount);
                        break;
                    }

                case DialogueActionType.AddGold:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        int amount = Mathf.Max(1, node.actionValue);
                        gm.AddMedal(Medal.Gold, amount);
                        break;
                    }

                case DialogueActionType.GoHomeEnding:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Loi: GameManager chua san sang.";
                            break;
                        }

                        StopDialogue();
                        gm.TriggerGoHomeEnding();
                        break;
                    }

                case DialogueActionType.CloseDialogue:
                    {
                        StopDialogue();
                        break;
                    }
            }
        }
    }
}