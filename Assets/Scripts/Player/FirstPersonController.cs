using UnityEngine;
using UnityEngine.InputSystem;

namespace FPTSim.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.81f;

        private CharacterController cc;
        private Vector3 velocity;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            // WASD using Keyboard
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
            }
            input = input.normalized;

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            cc.Move(move * moveSpeed * Time.deltaTime);

            // gravity
            if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
        }
    }
}