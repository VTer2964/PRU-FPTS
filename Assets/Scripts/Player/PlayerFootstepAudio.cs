using UnityEngine;
using FPTSim.Audio;

namespace FPTSim.Player
{
    public class PlayerFootstepAudio : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerBody;

        [Header("Footstep Clips")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField][Range(0f, 1f)] private float volume = 0.8f;

        [Header("Timing")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.32f;
        [SerializeField] private float movementThreshold = 0.1f;

        [Header("Ground Check")]
        [SerializeField] private bool requireGrounded = true;

        [Header("Optional Run Check")]
        [SerializeField] private bool useRunDetection = false;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

        private float stepTimer;

        private void Reset()
        {
            if (!characterController)
                characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;
            if (AudioManager.I == null) return;

            if (requireGrounded && characterController != null && !characterController.isGrounded)
            {
                stepTimer = 0f;
                return;
            }

            float moveAmount = GetHorizontalSpeed();

            if (moveAmount < movementThreshold)
            {
                stepTimer = 0f;
                return;
            }

            bool isRunning = useRunDetection && Input.GetKey(runKey);
            float interval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                stepTimer = 0f;
                PlayRandomFootstep();
            }
        }

        private float GetHorizontalSpeed()
        {
            if (characterController != null)
            {
                Vector3 vel = characterController.velocity;
                vel.y = 0f;
                return vel.magnitude;
            }

            if (playerBody != null)
            {
                return playerBody.GetComponent<Rigidbody>() != null
                    ? playerBody.GetComponent<Rigidbody>().linearVelocity.magnitude
                    : 0f;
            }

            return 0f;
        }

        private void PlayRandomFootstep()
        {
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[index];
            if (clip == null) return;

            AudioManager.I.PlaySfx(clip, volume);
        }
    }
}