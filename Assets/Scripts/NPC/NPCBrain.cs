using UnityEngine;
using UnityEngine.AI;

namespace FPTSim.NPC
{
    public class NPCBrain : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent; // có thể null nếu NPC đứng yên

        [Header("Animator Params (tùy bạn đặt tên)")]
        [SerializeField] private string speedParam = "Speed";     // Float 0..1
        [SerializeField] private string performBool = "Perform";  // Bool (lau nhà / làm việc)
        [SerializeField] private string talkBool = "Talk";        // Bool (khi nói chuyện)

        [Header("Face to face")]
        [SerializeField] private Transform facePivot; // null => quay toàn thân (transform)
        [SerializeField] private float rotateSpeed = 720f;

        private Transform lookTarget;
        private bool isTalking;

        // Save state để resume
        private bool hadAgent;
        private bool wasAgentStopped;
        private Vector3 savedDestination;
        private bool hadDestination;

        private bool wasPerforming;

        public bool IsTalking => isTalking;

        private void Awake()
        {
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!agent) agent = GetComponent<NavMeshAgent>();
            hadAgent = agent != null;
        }

        private void Update()
        {
            // cập nhật speed khi đang di chuyển và không talk
            if (!isTalking && animator != null && agent != null)
            {
                float v = agent.velocity.magnitude;
                float normalized = (agent.speed <= 0.01f) ? 0f : Mathf.Clamp01(v / agent.speed);
                if (!string.IsNullOrWhiteSpace(speedParam))
                    animator.SetFloat(speedParam, normalized);
            }

            // quay mặt nhìn player khi đang talk
            if (isTalking && lookTarget != null)
            {
                Transform pivot = facePivot ? facePivot : transform;

                Vector3 dir = lookTarget.position - pivot.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    pivot.rotation = Quaternion.RotateTowards(pivot.rotation, targetRot, rotateSpeed * Time.deltaTime);
                }
            }
        }

        // NPC đứng tại chỗ làm việc (lau nhà…) gọi cái này
        public void SetPerforming(bool performing)
        {
            wasPerforming = performing;

            if (animator != null && !string.IsNullOrWhiteSpace(performBool))
                animator.SetBool(performBool, performing);
        }

        // Khi bắt đầu nói chuyện
        public void EnterTalk(Transform player)
        {
            isTalking = true;
            lookTarget = player;

            // Save perform state
            bool curPerform = wasPerforming;

            // Stop agent nếu có
            if (hadAgent)
            {
                wasAgentStopped = agent.isStopped;

                if (agent.hasPath)
                {
                    savedDestination = agent.destination;
                    hadDestination = true;
                }
                else hadDestination = false;

                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Switch anim sang talk/idle
            if (animator != null)
            {
                if (!string.IsNullOrWhiteSpace(speedParam))
                    animator.SetFloat(speedParam, 0f);

                // tắt perform trong lúc talk
                if (!string.IsNullOrWhiteSpace(performBool))
                    animator.SetBool(performBool, false);

                if (!string.IsNullOrWhiteSpace(talkBool))
                    animator.SetBool(talkBool, true);
            }

            wasPerforming = curPerform; // nhớ lại để resume
        }

        // Khi kết thúc nói chuyện
        public void ExitTalk()
        {
            isTalking = false;
            lookTarget = null;

            if (animator != null)
            {
                if (!string.IsNullOrWhiteSpace(talkBool))
                    animator.SetBool(talkBool, false);

                // trả perform về trước đó
                if (!string.IsNullOrWhiteSpace(performBool))
                    animator.SetBool(performBool, wasPerforming);
            }

            // Resume agent nếu có
            if (hadAgent)
            {
                agent.isStopped = wasAgentStopped;

                if (!agent.isStopped && hadDestination)
                    agent.SetDestination(savedDestination);
            }
        }
    }
}