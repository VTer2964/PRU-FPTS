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

        [Header("Lock Player While Talking")]
        [SerializeField] private FPTSim.Player.MouseLook mouseLook;
        [SerializeField] private MonoBehaviour playerMovement;
        [SerializeField] private FPTSim.Player.Interactor playerInteractor;

        [Header("Input")]
        [SerializeField] private Key continueKey = Key.Space;
        [SerializeField] private Key exitKey = Key.Escape;

        private DialogueGraphSO currentGraph;
        private DialogueNodeSO currentNode;

        // runtime message (không ghi đè asset)
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

            // Có choices => bắt buộc chọn option, không cho click/Space next
            if (hasChoices) return;

            // Không có choices: chỉ cho continue nếu autoAdvance = true
            if (!currentNode.autoAdvance) return;

            bool pressContinue =
                (Keyboard.current != null && Keyboard.current[continueKey].wasPressedThisFrame) ||
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);

            if (pressContinue)
            {
                ContinueAuto();
            }
        }

        private void ContinueAuto()
        {
            if (currentNode == null) return;

            // nextAuto null => kết thúc dialogue
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

            // chạy action trước render
            if (node.triggerAction)
            {
                ExecuteAction(node);

                // action có thể StopDialogue / chuyển scene
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

            currentGraph = null;
            currentNode = null;
        }

        private void LockPlayer(bool talking)
        {
            // talking=true => unlock cursor + disable movement/interact
            if (mouseLook) mouseLook.LockCursor(!talking);
            if (playerMovement) playerMovement.enabled = !talking;
            if (playerInteractor) playerInteractor.enabled = !talking;
        }

        private void ExecuteAction(DialogueNodeSO node)
        {
            var gm = GameManager.I;

            switch (node.actionType)
            {
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
                        // nếu ok -> GameManager tự chuyển Ending
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