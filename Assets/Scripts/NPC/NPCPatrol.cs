using UnityEngine;
using UnityEngine.AI;

namespace FPTSim.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCPatrol : MonoBehaviour
    {
        [Header("Waypoints")]
        [SerializeField] private Transform[] points;
        [SerializeField] private float waitAtPoint = 1f;
        [SerializeField] private bool loop = true;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string speedParam = "Speed";

        private NavMeshAgent agent;
        private int index;
        private float waitTimer;
        private bool waiting;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (points != null && points.Length > 0)
                GoTo(points[0]);
        }

        private void Update()
        {
            UpdateAnimator();

            if (points == null || points.Length == 0) return;

            if (waiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    waiting = false;
                    NextPoint();
                }
                return;
            }

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                waiting = true;
                waitTimer = waitAtPoint;
                agent.isStopped = true;
            }
        }

        private void NextPoint()
        {
            agent.isStopped = false;

            index++;
            if (index >= points.Length)
            {
                if (!loop) return;
                index = 0;
            }
            GoTo(points[index]);
        }

        private void GoTo(Transform t)
        {
            if (t == null) return;
            agent.SetDestination(t.position);
        }

        private void UpdateAnimator()
        {
            if (!animator) return;

            float v = agent.velocity.magnitude;
            float normalized = Mathf.InverseLerp(0f, agent.speed, v); // 0..1
            animator.SetFloat(speedParam, normalized);
        }

        // Cho hệ cốt truyện gọi
        public void SetWaypoints(Transform[] newPoints, bool resetIndex = true)
        {
            points = newPoints;
            if (resetIndex) index = 0;
            waiting = false;
            agent.isStopped = false;
            if (points != null && points.Length > 0) GoTo(points[index]);
        }

        public void StopPatrol()
        {
            agent.isStopped = true;
            waiting = false;
        }
    }
}