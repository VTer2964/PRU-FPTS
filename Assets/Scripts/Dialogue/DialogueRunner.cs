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

        private DialogueGraphSO currentGraph;
        private DialogueNodeSO currentNode;
        private string runtimeMessage;

        public bool IsRunning => currentGraph != null;

        public System.Action OnDialogueStopped;

        public void StartDialogue(DialogueGraphSO graph)
        {
            if (graph == null || graph.entryNode == null) return;
            if (ui == null)
            {
                Debug.LogError("[DialogueRunner] Missing DialogueUI_Genshin reference!");
                return;
            }

            currentGraph = graph;
            currentNode = graph.entryNode;

            ui.Open(OnChoiceSelected, StopDialogue);
            LockPlayer(true);

            ShowNode(currentNode);
        }

        private void Update()
        {
            if (!IsRunning) return;

            if (Keyboard.current != null && Keyboard.current[exitKey].wasPressedThisFrame)
            {
                StopDialogue();
                return;
            }

            if (currentNode == null) return;

            bool pressContinue =
                (Keyboard.current != null && Keyboard.current[continueKey].wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (!pressContinue) return;

            // Nếu đang typewriter -> complete câu hiện tại trước
            if (ui != null && ui.IsTyping)
            {
                ui.CompleteTyping();
                return;
            }

            bool hasChoices = currentNode.choices != null && currentNode.choices.Length > 0;

            // Có choices -> phải chọn option, không cho skip tiếp
            if (hasChoices) return;

            // Không có choices -> nếu autoAdvance thì qua node tiếp
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

        private void ShowNode(DialogueNodeSO node)
        {
            if (node == null)
            {
                StopDialogue();
                return;
            }

            runtimeMessage = null;

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
                            runtimeMessage = "Lỗi: chưa gán DialogueCameraController.";
                            break;
                        }

                        string key = node.actionParam != null ? node.actionParam.Trim() : "";
                        if (string.IsNullOrWhiteSpace(key))
                        {
                            runtimeMessage = "Lỗi: SetCamera thiếu actionParam (cameraKey).";
                            break;
                        }

                        bool ok = camCtrl.Focus(key);
                        if (!ok) runtimeMessage = $"Lỗi: không tìm thấy camera key '{key}'.";
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

                        if (gm == null)
                        {
                            Debug.LogError("[DialogueRunner] GameManager is null.");
                            return;
                        }

                        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                        if (playerObj == null)
                        {
                            Debug.LogError("[DialogueRunner] Không tìm thấy Player có tag Player.");
                            return;
                        }

                        gm.EnterMinigame(scene, playerObj.transform);
                        break;
                    }

                case DialogueActionType.BuyTimeWithBronze:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithBronze();
                        runtimeMessage = ok
                            ? "OK rồi em nhá"
                            : "Không đủ huy chương đồng?";
                        break;
                    }

                case DialogueActionType.BuyTimeWithSilver:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithSilver();
                        runtimeMessage = ok
                            ? "OK rồi em nhá"
                            : "Không đủ huy chương bạc";
                        break;
                    }

                case DialogueActionType.BuyTimeWithGold:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithGold();
                        runtimeMessage = ok
                            ? "OK rồi em nhá"
                            : "Không đủ huy chương vàng";
                        break;
                    }

                case DialogueActionType.SubmitToWin:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
                            break;
                        }

                        bool ok = gm.TrySubmitToWin();
                        if (!ok)
                        {
                            var c = gm.Config;
                            runtimeMessage =
                                $"❌ Chưa đủ huy chương yêu cầu!\n" +
                                $"Cần: G{c.requiredGold}  S{c.requiredSilver}  B{c.requiredBronze}";
                        }
                        break;
                    }

                case DialogueActionType.SetFlag:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
                            break;
                        }

                        string flag = node.actionParam != null ? node.actionParam.Trim() : "";
                        if (string.IsNullOrWhiteSpace(flag))
                        {
                            runtimeMessage = "Lỗi: SetFlag thiếu actionParam (tên flag).";
                            break;
                        }

                        gm.SetFlag(flag);
                        break;
                    }

                case DialogueActionType.AddBronze:
                    {
                        if (gm == null)
                        {
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
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
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
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
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
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
                            runtimeMessage = "Lỗi: GameManager chưa sẵn sàng.";
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