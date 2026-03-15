using UnityEngine;
using UnityEngine.AI;

namespace FPTSim.NPC
{
    public class NPCBrain : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private NPCPatrol patrol;

        [Header("Animator Params")]
        [SerializeField] private string speedParam = "Speed";
        [SerializeField] private string performBool = "Perform";
        [SerializeField] private string talkBool = "Talk";

        [Header("Movement")]
        [SerializeField] private float movingDistanceBuffer = 0.05f;
        [SerializeField] private float movingVelocityThreshold = 0.05f;

        [Header("Face to face")]
        [SerializeField] private Transform facePivot;
        [SerializeField] private float rotateSpeed = 720f;

        [Header("Return facing after talk")]
        [SerializeField] private bool returnToOriginalFacing = true;
        [SerializeField] private float returnRotateSpeed = 720f;

        private Transform lookTarget;
        private bool isTalking;
        private bool requestedPerforming;

        private bool hadAgent;
        private bool wasAgentStopped;
        private Vector3 savedDestination;
        private bool hadDestination;

        private Quaternion savedFacing;
        private bool hasSavedFacing;
        private bool isReturningFacing;

        public bool IsTalking => isTalking;
        private Transform Pivot => facePivot ? facePivot : transform;

        private void Awake()
        {
            if (!animator) animator = GetComponentInChildren<Animator>();
            if (!agent) agent = GetComponent<NavMeshAgent>();
            if (!patrol) patrol = GetComponent<NPCPatrol>();
            hadAgent = agent != null;
        }

        private void Update()
        {
            RefreshAnimatorState();

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
            requestedPerforming = performing;
            RefreshAnimatorState();
        }

        public void EnterTalk(Transform player)
        {
            isTalking = true;
            lookTarget = player;

            hasSavedFacing = true;
            savedFacing = Pivot.rotation;
            isReturningFacing = false;

            if (patrol != null)
                patrol.PausePatrol();

            if (hadAgent)
            {
                wasAgentStopped = agent.isStopped;

                if (agent.hasPath)
                {
                    savedDestination = agent.destination;
                    hadDestination = true;
                }
                else
                {
                    hadDestination = false;
                }

                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            RefreshAnimatorState(0f);
        }

        public void ExitTalk()
        {
            isTalking = false;
            lookTarget = null;

            if (returnToOriginalFacing && hasSavedFacing)
                isReturningFacing = true;

            if (hadAgent)
            {
                agent.isStopped = wasAgentStopped;

                if (!agent.isStopped && hadDestination)
                    agent.SetDestination(savedDestination);
            }

            if (patrol != null)
                patrol.ResumePatrol();

            RefreshAnimatorState();
        }

        private void RefreshAnimatorState(float? normalizedSpeedOverride = null)
        {
            if (animator == null) return;

            float normalizedSpeed = normalizedSpeedOverride ?? GetNormalizedSpeed();
            bool moving = !isTalking && IsAgentMoving();
            bool performActive = requestedPerforming && !moving && !isTalking;

            if (!string.IsNullOrWhiteSpace(speedParam))
                animator.SetFloat(speedParam, isTalking ? 0f : normalizedSpeed);

            if (!string.IsNullOrWhiteSpace(performBool))
                animator.SetBool(performBool, performActive);

            if (!string.IsNullOrWhiteSpace(talkBool))
                animator.SetBool(talkBool, isTalking);
        }

        private float GetNormalizedSpeed()
        {
            if (agent == null || !agent.enabled) return 0f;
            if (agent.speed <= 0.01f) return 0f;

            float speed = agent.velocity.magnitude;
            return Mathf.Clamp01(speed / agent.speed);
        }

        private bool IsAgentMoving()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh) return false;
            if (agent.isStopped) return false;
            if (agent.velocity.magnitude > movingVelocityThreshold) return true;
            if (agent.pathPending) return true;
            if (!agent.hasPath) return false;

            return agent.remainingDistance > agent.stoppingDistance + movingDistanceBuffer;
        }
    }
}
