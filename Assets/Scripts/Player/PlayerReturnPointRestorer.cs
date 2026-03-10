using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Player
{
    public class PlayerReturnPointRestorer : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;

        private void Start()
        {
            if (GameManager.I == null) return;
            if (!GameManager.I.HasReturnPoint) return;

            // Tắt CharacterController tạm nếu có để tránh teleport lỗi
            bool hadCC = characterController != null;
            if (hadCC) characterController.enabled = false;

            transform.position = GameManager.I.ReturnPlayerPosition;
            transform.rotation = GameManager.I.ReturnPlayerRotation;

            if (hadCC) characterController.enabled = true;

            GameManager.I.ClearReturnPoint();
        }
    }
}