using UnityEngine;
using UnityEngine.AI;
using FPTSim.Core;

namespace FPTSim.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCStoryRoute : MonoBehaviour
    {
        [System.Serializable]
        public class Step
        {
            [Header("Khi co flag nay thi NPC di toi destination")]
            public string requiredFlag;

            [Header("Neu co flag nay thi step route se tat va tra NPC ve patrol")]
            public string stopWhenFlagPresent;

            [Header("Diem den")]
            public Transform destination;

            [Header("Optional: toi noi thi set flag nay")]
            public string setFlagOnArrive;
        }

        [Header("Route Steps")]
        [SerializeField] private Step[] steps;
        [SerializeField] private float arriveDistance = 0.6f;

        [Header("Refs")]
        [SerializeField] private NPCPatrol patrol;
        [SerializeField] private NPCBrain brain;
        [SerializeField] private NavMeshAgent agent;

        [Header("Patrol takeover")]
        [SerializeField] private bool suppressPatrolWhileRouteActive = true;

        [Header("Perform behaviour")]
        [SerializeField] private bool resumePerformWhenArrived = true;
        [SerializeField] private bool stopPerformWhenMoving = true;

        private int activeStep = -1;
        private bool arrivedFlagFired;
        private bool hasReachedDestination;
        private bool patrolSuppressedByRoute;

        private void Awake()
        {
            if (!agent) agent = GetComponent<NavMeshAgent>();
            if (!brain) brain = GetComponent<NPCBrain>();
            if (!patrol) patrol = GetComponent<NPCPatrol>();
        }

        private void Update()
        {
            if (GameManager.I == null) return;
            if (steps == null || steps.Length == 0) return;
            if (agent == null || !agent.enabled) return;

            int best = GetBestStep();
            if (best == -1)
            {
                ClearActiveStep();
                return;
            }

            if (best != activeStep)
                ActivateStep(best);
            else
                ApplyRouteSuppression(true);

            if (activeStep < 0 || activeStep >= steps.Length) return;

            Step step = steps[activeStep];
            if (step == null || step.destination == null) return;
            if (brain != null && brain.IsTalking) return;

            EnsureHeadingToActiveDestination(step);

            bool isMoving =
                agent.enabled &&
                agent.isOnNavMesh &&
                !agent.isStopped &&
                (agent.pathPending || agent.remainingDistance > Mathf.Max(arriveDistance, agent.stoppingDistance));

            if (isMoving)
            {
                hasReachedDestination = false;

                if (brain != null)
                {
                    brain.ClearIdleFacingOverride();

                    if (stopPerformWhenMoving)
                        brain.SetPerforming(false);
                }

                return;
            }

            if (brain != null)
                brain.SetIdleFacingDirection(step.destination.forward);

            if (!hasReachedDestination)
            {
                hasReachedDestination = true;

                if (resumePerformWhenArrived && brain != null)
                    brain.SetPerforming(true);

                if (!arrivedFlagFired && !string.IsNullOrWhiteSpace(step.setFlagOnArrive))
                {
                    arrivedFlagFired = true;
                    GameManager.I.SetFlag(step.setFlagOnArrive);
                }
            }
        }

        private void ActivateStep(int stepIndex)
        {
            activeStep = stepIndex;
            arrivedFlagFired = false;
            hasReachedDestination = false;

            ApplyRouteSuppression(true);

            if (brain != null)
            {
                brain.ClearIdleFacingOverride();

                if (stopPerformWhenMoving)
                    brain.SetPerforming(false);
            }

            if (brain != null && brain.IsTalking) return;

            Step step = steps[activeStep];
            if (step == null || step.destination == null) return;

            EnsureHeadingToActiveDestination(step);
        }

        private void ClearActiveStep()
        {
            if (activeStep == -1 && !patrolSuppressedByRoute) return;

            activeStep = -1;
            arrivedFlagFired = false;
            hasReachedDestination = false;
            ApplyRouteSuppression(false);

            if (brain != null)
                brain.ClearIdleFacingOverride();
        }

        private void EnsureHeadingToActiveDestination(Step step)
        {
            if (step == null || step.destination == null) return;
            if (!agent.isOnNavMesh) return;

            Vector3 target = step.destination.position;
            bool shouldRefreshDestination =
                agent.isStopped ||
                !agent.hasPath ||
                Vector3.Distance(agent.destination, target) > 0.05f;

            if (!shouldRefreshDestination) return;

            agent.isStopped = false;
            agent.SetDestination(target);
        }

        private void ApplyRouteSuppression(bool suppress)
        {
            if (!suppressPatrolWhileRouteActive || patrol == null) return;
            if (patrolSuppressedByRoute == suppress) return;

            if (suppress)
                patrol.SuppressPatrol();
            else
                patrol.UnsuppressPatrol();

            patrolSuppressedByRoute = suppress;
        }

        private int GetBestStep()
        {
            int best = -1;

            for (int i = 0; i < steps.Length; i++)
            {
                Step step = steps[i];
                if (!IsStepActive(step)) continue;

                best = i;
            }

            return best;
        }

        private bool IsStepActive(Step step)
        {
            if (step == null) return false;
            if (string.IsNullOrWhiteSpace(step.requiredFlag)) return false;
            if (!GameManager.I.HasFlag(step.requiredFlag)) return false;

            if (!string.IsNullOrWhiteSpace(step.stopWhenFlagPresent) &&
                GameManager.I.HasFlag(step.stopWhenFlagPresent))
            {
                return false;
            }

            return true;
        }
    }
}