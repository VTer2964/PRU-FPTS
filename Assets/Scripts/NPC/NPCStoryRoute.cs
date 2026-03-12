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
            [Header("Khi có flag này thì NPC đi tới destination")]
            public string requiredFlag;

            [Header("Điểm đến")]
            public Transform destination;

            [Header("Optional: tới nơi thì set flag này")]
            public string setFlagOnArrive;
        }

        [Header("Route Steps")]
        [SerializeField] private Step[] steps;
        [SerializeField] private float arriveDistance = 0.6f;

        [Header("Refs")]
        [SerializeField] private NPCPatrol patrol;
        [SerializeField] private NPCBrain brain;
        [SerializeField] private NavMeshAgent agent;

        [Header("Perform behaviour")]
        [SerializeField] private bool resumePerformWhenArrived = true;
        [SerializeField] private bool stopPerformWhenMoving = true;

        private int activeStep = -1;
        private bool arrivedFlagFired;
        private bool hasReachedDestination;

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
            if (agent == null) return;

            int best = GetBestStep();
            if (best == -1) return;

            // Có step mới được kích hoạt
            if (best != activeStep)
            {
                activeStep = best;
                arrivedFlagFired = false;
                hasReachedDestination = false;

                var s = steps[activeStep];
                if (s.destination != null)
                {
                    if (patrol != null)
                        patrol.StopPatrol();

                    if (stopPerformWhenMoving && brain != null)
                        brain.SetPerforming(false);

                    agent.isStopped = false;
                    agent.SetDestination(s.destination.position);
                }
            }

            var step = steps[activeStep];
            if (step.destination == null) return;

            // Nếu đang đi
            bool isMoving =
                agent.enabled &&
                !agent.isStopped &&
                (agent.pathPending || agent.remainingDistance > Mathf.Max(arriveDistance, agent.stoppingDistance));

            if (isMoving)
            {
                hasReachedDestination = false;
                return;
            }

            // Đã tới nơi
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

        private int GetBestStep()
        {
            int best = -1;

            for (int i = 0; i < steps.Length; i++)
            {
                var f = steps[i].requiredFlag;
                if (string.IsNullOrWhiteSpace(f)) continue;

                if (GameManager.I.HasFlag(f))
                    best = i; // step sau ưu tiên hơn step trước
            }

            return best;
        }
    }
}