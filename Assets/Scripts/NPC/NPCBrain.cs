using UnityEngine;
using UnityEngine.AI;

namespace FPTSim.NPC
{
    public class NPCBrain : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;

        [Header("Animator Params")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string performBool = "Perform";
        [SerializeField] private string talkBool = "Talk";

        [Header("Face to face")]
        [SerializeField] private Transform facePivot;
        [SerializeField] private float rotateSpeed = 720f;

        [Header("Return facing after talk")]
        [SerializeField] private bool returnToOriginalFacing = true;
        [SerializeField] private float returnRotateSpeed = 720f;

        private Transform lookTarget;
        private bool isTalking;

        private bool hadAgent;
        private bool wasAgentStopped;
        private Vector3 savedDestination;
        private bool hadDestination;

        private bool wasPerforming;
        private bool wasMovingBeforeTalk;   // ✅ thêm

        private Quaternion savedFacing;
        private bool hasSavedFacing;
        private bool isReturningFacing;

        public bool IsTalking => isTalking;
        private Transform Pivot => facePivot ? facePivot : transform;

        private void Awake()
        {
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!agent) agent = GetComponent<NavMeshAgent>();
            hadAgent = agent != null;
        }

        private void Update()
        {
            if (!isTalking && animator != null && agent != null)
            {
                float v = agent.velocity.magnitude;
                float normalized = (agent.speed <= 0.01f) ? 0f : Mathf.Clamp01(v / agent.speed);

                if (!string.IsNullOrWhiteSpace(speedParam))
                    animator.SetFloat(speedParam, normalized);
            }

            if (isTalking && lookTarget != null)
            {
                isReturningFacing = false;

                Vector3 dir = lookTarget.position - Pivot.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, targetRot, rotateSpeed * Time.deltaTime);
                }
            }
            else if (!isTalking && returnToOriginalFacing && isReturningFacing && hasSavedFacing)
            {
                Pivot.rotation = Quaternion.RotateTowards(Pivot.rotation, savedFacing, returnRotateSpeed * Time.deltaTime);

                if (Quaternion.Angle(Pivot.rotation, savedFacing) < 0.5f)
                {
                    Pivot.rotation = savedFacing;
                    isReturningFacing = false;
                }
            }
        }

        public void SetPerforming(bool performing)
        {
            wasPerforming = performing;

            if (animator != null && !string.IsNullOrWhiteSpace(performBool))
                animator.SetBool(performBool, performing);
        }

        public void EnterTalk(Transform player)
        {
            isTalking = true;
            lookTarget = player;

            hasSavedFacing = true;
            savedFacing = Pivot.rotation;

            // ✅ nhớ xem trước khi talk có đang di chuyển không
            wasMovingBeforeTalk = false;

            if (hadAgent)
            {
                wasAgentStopped = agent.isStopped;

                if (agent.hasPath)
                {
                    savedDestination = agent.destination;
                    hadDestination = true;

                    // Nếu đang có path và chưa tới nơi thì coi như đang đi
                    if (!agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.05f)
                        wasMovingBeforeTalk = true;
                    else if (agent.pathPending)
                        wasMovingBeforeTalk = true;
                }
                else
                {
                    hadDestination = false;
                }

                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            if (animator != null)
            {
                if (!string.IsNullOrWhiteSpace(speedParam))
                    animator.SetFloat(speedParam, 0f);

                if (!string.IsNullOrWhiteSpace(performBool))
                    animator.SetBool(performBool, false);

                if (!string.IsNullOrWhiteSpace(talkBool))
                    animator.SetBool(talkBool, true);
            }
        }

        public void ExitTalk()
        {
            isTalking = false;
            lookTarget = null;

            if (returnToOriginalFacing && hasSavedFacing)
                isReturningFacing = true;

            if (animator != null)
            {
                if (!string.IsNullOrWhiteSpace(talkBool))
                    animator.SetBool(talkBool, false);
            }

            // Resume agent trước
            if (hadAgent)
            {
                agent.isStopped = wasAgentStopped;

                if (!agent.isStopped && hadDestination)
                    agent.SetDestination(savedDestination);
            }

            // ✅ CHỈ bật lại Perform nếu trước đó KHÔNG phải đang di chuyển
            if (animator != null && !string.IsNullOrWhiteSpace(performBool))
            {
                if (!wasMovingBeforeTalk)
                    animator.SetBool(performBool, wasPerforming);
                else
                    animator.SetBool(performBool, false);
            }
        }
    }
}