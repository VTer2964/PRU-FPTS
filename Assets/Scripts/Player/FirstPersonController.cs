using UnityEngine;
using UnityEngine.InputSystem;

namespace FPTSim.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Move")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float runSpeed = 6f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Animation")]
        [SerializeField] private Animator animator; // Animator nằm trên object Model (con của Player)

        private CharacterController cc;
        private Vector3 velocity;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // Input WASD
            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) input.y += 1;
                if (Keyboard.current.sKey.isPressed) input.y -= 1;
                if (Keyboard.current.aKey.isPressed) input.x -= 1;
                if (Keyboard.current.dKey.isPressed) input.x += 1;
            }
            input = input.normalized;

            // Sprint (Shift)
            bool isRunning = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            float speedNow = isRunning ? runSpeed : walkSpeed;

            Vector3 move = transform.right * input.x + transform.forward * input.y;
            cc.Move(move * speedNow * Time.deltaTime);

            // Animation Speed (0..runSpeed)
            if (animator != null)
            {
                float animSpeed = input.magnitude * speedNow;
                animator.SetFloat("Speed", animSpeed);
            }

            // Gravity
            if (cc.isGrounded && velocity.y < 0) velocity.y = -2f;
            velocity.y += gravity * Time.deltaTime;
            cc.Move(velocity * Time.deltaTime);
        }
    }
}