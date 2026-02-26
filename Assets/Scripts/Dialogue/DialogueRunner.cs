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
        [SerializeField] private FPTSim.Player.MouseLook mouseLook; // MouseLook trên CameraPivot
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;

        [Header("Input")]
        [SerializeField] private Key continueKey = Key.Space;
        [SerializeField] private Key exitKey = Key.Escape;

        private DialogueGraphSO currentGraph;
        private DialogueNodeSO currentNode;
        private string runtimeMessage;

        public bool IsRunning => currentGraph != null;

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

            // ESC luôn thoát
            if (Keyboard.current != null && Keyboard.current[exitKey].wasPressedThisFrame)
            {
                StopDialogue();
                return;
            }

            if (currentNode == null) return;

            bool hasChoices = currentNode.choices != null && currentNode.choices.Length > 0;
            if (hasChoices) return;

            if (!currentNode.autoAdvance) return;

            bool pressContinue =
                (Keyboard.current != null && Keyboard.current[continueKey].wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (pressContinue) ContinueAuto();
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
                if (currentGraph == null) return; // có thể StopDialogue / load scene
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

            // next null => thoát
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

            // ✅ hạ priority cam cutscene/dialogue => tự về CM_Gameplay
            camCtrl?.Clear();

            currentGraph = null;
            currentNode = null;
        }

        private void LockPlayer(bool talking)
        {
            // talking=true => unlock cursor + disable movement/interact + disable look
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

                        SceneManager.LoadScene(scene);
                        break;
                    }

                case DialogueActionType.BuyTimeWithBronze:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithBronze();
                        runtimeMessage = ok
                            ? "✅ Đổi thành công! Bạn được cộng thêm thời gian."
                            : "❌ Bạn không đủ Bronze để mua thêm thời gian!";
                        break;
                    }

                case DialogueActionType.BuyTimeWithSilver:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithSilver();
                        runtimeMessage = ok
                            ? "✅ Đổi thành công! Bạn được cộng thêm thời gian."
                            : "❌ Bạn không đủ Silver để mua thêm thời gian!";
                        break;
                    }

                case DialogueActionType.BuyTimeWithGold:
                    {
                        bool ok = gm != null && gm.TryBuyTimeWithGold();
                        runtimeMessage = ok
                            ? "✅ Đổi thành công! Bạn được cộng thêm thời gian."
                            : "❌ Bạn không đủ Gold để mua thêm thời gian!";
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
                        runtimeMessage = $"✅ Đã mở khóa cốt truyện: {flag}";
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