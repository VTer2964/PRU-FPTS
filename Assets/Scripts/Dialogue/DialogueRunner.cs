using UnityEngine;
using UnityEngine.SceneManagement;
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

        private DialogueGraphSO currentGraph;
        private DialogueNodeSO currentNode;

        // Thông báo runtime (không ghi đè asset node.text)
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

        private void ShowNode(DialogueNodeSO node)
        {
            if (node == null)
            {
                StopDialogue();
                return;
            }

            runtimeMessage = null;

            // chạy action trước khi render
            if (node.triggerAction)
            {
                ExecuteAction(node);

                // nếu action đã StopDialogue() (vd StartMinigameScene/CloseDialogue)
                if (currentGraph == null) return;
            }

            ui.RenderNode(node);

            // nếu có message runtime (fail/success), ghi đè body sau khi render
            if (!string.IsNullOrWhiteSpace(runtimeMessage))
            {
                ui.SetBodyText(runtimeMessage);
            }
        }

        private void OnChoiceSelected(int choiceIndex)
        {
            if (currentNode == null || currentNode.choices == null) return;
            if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Length) return;

            var choice = currentNode.choices[choiceIndex];

            // ✅ choice.next null => kết thúc hội thoại
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

                        // Nếu ok -> GameManager sẽ tự chuyển Ending ngay
                        if (!ok)
                        {
                            var c = gm.Config;
                            runtimeMessage =
                                $"❌ Chưa đủ huy chương yêu cầu!\n" +
                                $"Cần: G{c.requiredGold}  S{c.requiredSilver}  B{c.requiredBronze}";
                        }

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