using UnityEngine;
using UnityEngine.InputSystem;

namespace FPTSim.Player
{
    public class MouseLook : MonoBehaviour
    {
        [SerializeField] private Transform playerBody;
        [SerializeField] private float sensitivity = 120f;

        private float xRotation;
        private bool canLook = true;

        private void Start()
        {
            LockCursor(true);
        }

        private void Update()
        {
            if (!canLook) return;
            if (Mouse.current == null) return;

            Vector2 delta = Mouse.current.delta.ReadValue();
            float mouseX = delta.x * sensitivity * Time.deltaTime;
            float mouseY = delta.y * sensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -85f, 85f);

            // Pitch (nhìn lên/xuống) cho pivot
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Yaw (quay trái/phải) cho thân player
            if (playerBody) playerBody.Rotate(Vector3.up * mouseX);
        }

        public void LockCursor(bool locked)
        {
            canLook = locked;

            if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}