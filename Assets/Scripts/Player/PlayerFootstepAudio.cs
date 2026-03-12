using UnityEngine;
using FPTSim.Audio;

namespace FPTSim.Player
{
    public class PlayerFootstepAudio : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform playerBody;
        [SerializeField] private AudioSource fallbackSource;

        [Header("Footstep Clips")]
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField][Range(0f, 1f)] private float volume = 0.8f;

        [Header("Timing")]
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.32f;
        [SerializeField] private float movementThreshold = 0.1f;

        [Header("Ground Check")]
        [SerializeField] private bool requireGrounded = true;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundProbeDistance = 0.25f;
        [SerializeField] private float groundedGraceTime = 0.1f;

        [Header("Optional Run Check")]
        [SerializeField] private bool useRunDetection = false;
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

        private float stepTimer;
        private float lastGroundedTime;
        private Vector3 lastPosition;

        private void Reset()
        {
            if (!characterController)
                characterController = GetComponent<CharacterController>();

            if (!fallbackSource)
            {
                fallbackSource = GetComponent<AudioSource>();
                if (!fallbackSource) fallbackSource = gameObject.AddComponent<AudioSource>();
                fallbackSource.playOnAwake = false;
                fallbackSource.spatialBlend = 0f;
            }
        }

        private void Awake()
        {
            if (!characterController)
                characterController = GetComponent<CharacterController>();

            if (!fallbackSource)
            {
                fallbackSource = GetComponent<AudioSource>();
                if (!fallbackSource) fallbackSource = gameObject.AddComponent<AudioSource>();
                fallbackSource.playOnAwake = false;
                fallbackSource.spatialBlend = 0f;
            }

            lastPosition = transform.position;
            lastGroundedTime = Time.time;
        }

        private void Update()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;

            if (IsGrounded())
                lastGroundedTime = Time.time;

            if (requireGrounded && Time.time - lastGroundedTime > groundedGraceTime)
            {
                stepTimer = 0f;
                lastPosition = transform.position;
                return;
            }

            float moveAmount = GetHorizontalSpeed();

            if (moveAmount < movementThreshold)
            {
                stepTimer = 0f;
                lastPosition = transform.position;
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

            lastPosition = transform.position;
        }

        private float GetHorizontalSpeed()
        {
            float speedFromDisplacement = 0f;
            if (Time.deltaTime > 0f)
            {
                Vector3 frameDelta = transform.position - lastPosition;
                frameDelta.y = 0f;
                speedFromDisplacement = frameDelta.magnitude / Time.deltaTime;
            }

            if (characterController != null)
            {
                Vector3 vel = characterController.velocity;
                vel.y = 0f;
                return Mathf.Max(vel.magnitude, speedFromDisplacement);
            }

            if (playerBody != null)
            {
                Rigidbody rb = playerBody.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 vel = rb.linearVelocity;
                    vel.y = 0f;
                    return Mathf.Max(vel.magnitude, speedFromDisplacement);
                }
            }

            return speedFromDisplacement;
        }

        private bool IsGrounded()
        {
            if (characterController != null && characterController.isGrounded)
                return true;

            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float radius = characterController != null ? Mathf.Max(0.05f, characterController.radius * 0.9f) : 0.2f;
            float distance = Mathf.Max(0.05f, groundProbeDistance);
            return Physics.SphereCast(origin, radius, Vector3.down, out _, distance, groundMask, QueryTriggerInteraction.Ignore);
        }

        private void PlayRandomFootstep()
        {
            int index = Random.Range(0, footstepClips.Length);
            AudioClip clip = footstepClips[index];
            if (clip == null) return;

            if (AudioManager.I != null)
            {
                AudioManager.I.PlaySfx(clip, volume);
                return;
            }

            if (fallbackSource != null)
                fallbackSource.PlayOneShot(clip, volume);
        }
    }
}
