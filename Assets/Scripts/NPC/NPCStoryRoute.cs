using UnityEngine;
using UnityEngine.AI;
using FPTSim.Core;

namespace FPTSim.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCStoryRoute : MonoBehaviour
    {
        [System.Serializable]
        public class RouteStep
        {
            public string requiredFlag;      // ví dụ: "A_TALK_DONE"
            public Transform destination;    // điểm A
            public string setFlagOnArrive;   // ví dụ: "A_AT_POINT_A" (optional)
        }

        [SerializeField] private RouteStep[] steps;
        [SerializeField] private float arriveDistance = 0.6f;

        private NavMeshAgent agent;
        private int currentStep = -1;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (GameManager.I == null) return;

            // 1) tìm step mới cần chạy theo flag
            int targetStep = GetHighestAvailableStep();
            if (targetStep != -1 && targetStep != currentStep)
            {
                currentStep = targetStep;
                var step = steps[currentStep];
                if (step.destination != null)
                    agent.SetDestination(step.destination.position);
            }

            // 2) nếu đang chạy step, check đến nơi
            if (currentStep >= 0 && currentStep < steps.Length)
            {
                var step = steps[currentStep];
                if (step.destination == null) return;

                if (!agent.pathPending && agent.remainingDistance <= arriveDistance)
                {
                    if (!string.IsNullOrWhiteSpace(step.setFlagOnArrive))
                        GameManager.I.SetFlag(step.setFlagOnArrive);
                }
            }
        }

        private int GetHighestAvailableStep()
        {
            if (steps == null || steps.Length == 0) return -1;

            int best = -1;
            for (int i = 0; i < steps.Length; i++)
            {
                string f = steps[i].requiredFlag;
                if (string.IsNullOrWhiteSpace(f)) continue;

                if (GameManager.I.HasFlag(f))
                    best = i;
            }
            return best;
        }
    }
}