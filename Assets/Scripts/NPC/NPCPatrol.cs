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
        private bool patrolEnabled = true;
        private int pauseRequests;
        private int suppressRequests;

        public bool IsPatrolEnabled => patrolEnabled;
        public bool IsPaused => pauseRequests > 0;
        public bool IsSuppressed => suppressRequests > 0;
        public bool IsPatrolActive => patrolEnabled && !IsPaused && !IsSuppressed;

        private bool HasWaypoints => points != null && points.Length > 0;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            if (HasWaypoints)
            {
                index = Mathf.Clamp(index, 0, points.Length - 1);
                if (IsPatrolActive)
                    BeginPatrolFromCurrentIndex();
                else
                    HoldPosition();
            }
        }

        private void Update()
        {
            if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            {
                UpdateAnimatorImmediate(0f);
                return;
            }

            UpdateAnimator();

            if (!IsPatrolActive) return;
            if (!HasWaypoints) return;

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
                agent.velocity = Vector3.zero;
            }
        }

        private void NextPoint()
        {
            if (!HasWaypoints)
            {
                HoldPosition();
                return;
            }

            index++;
            if (index >= points.Length)
            {
                if (!loop)
                {
                    index = points.Length - 1;
                    HoldPosition();
                    return;
                }

                index = 0;
            }

            agent.isStopped = false;
            GoTo(points[index]);
        }

        private void GoTo(Transform targetPoint)
        {
            if (targetPoint == null || agent == null || !agent.enabled || !agent.isOnNavMesh) return;
            agent.SetDestination(targetPoint.position);
        }

        private void UpdateAnimator()
        {
            if (!animator || string.IsNullOrWhiteSpace(speedParam) || agent == null) return;

            float speed = agent.velocity.magnitude;
            float normalized = Mathf.InverseLerp(0f, Mathf.Max(0.01f, agent.speed), speed);
            animator.SetFloat(speedParam, normalized);
        }

        private void UpdateAnimatorImmediate(float normalizedSpeed)
        {
            if (!animator || string.IsNullOrWhiteSpace(speedParam)) return;
            animator.SetFloat(speedParam, normalizedSpeed);
        }

        public void SetWaypoints(Transform[] newPoints, bool resetIndex = true)
        {
            points = newPoints;
            if (resetIndex) index = 0;

            waiting = false;
            waitTimer = 0f;

            if (IsPatrolActive)
                BeginPatrolFromCurrentIndex();
            else
                HoldPosition();
        }

        public void SetPatrolEnabled(bool enabled)
        {
            if (patrolEnabled == enabled) return;

            patrolEnabled = enabled;
            waiting = false;
            waitTimer = 0f;

            if (IsPatrolActive)
                BeginPatrolFromCurrentIndex();
            else
                HoldPosition();
        }

        public void PausePatrol()
        {
            pauseRequests++;
            HoldPosition();
        }

        public void ResumePatrol()
        {
            if (pauseRequests <= 0) return;

            pauseRequests--;
            if (IsPatrolActive)
                ResumePatrolMovement();
        }

        public void SuppressPatrol()
        {
            suppressRequests++;
            HoldPosition();
        }

        public void UnsuppressPatrol()
        {
            if (suppressRequests <= 0) return;

            suppressRequests--;
            if (IsPatrolActive)
                ResumePatrolMovement();
        }

        public void StopPatrol()
        {
            SetPatrolEnabled(false);
        }

        private void BeginPatrolFromCurrentIndex()
        {
            if (!HasWaypoints)
            {
                HoldPosition();
                return;
            }

            index = Mathf.Clamp(index, 0, points.Length - 1);
            waiting = false;
            waitTimer = 0f;
            agent.isStopped = false;
            GoTo(points[index]);
        }

        private void ResumePatrolMovement()
        {
            if (!HasWaypoints)
            {
                HoldPosition();
                return;
            }

            if (waiting)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                UpdateAnimatorImmediate(0f);
                return;
            }

            agent.isStopped = false;
            GoTo(points[index]);
        }

        private void HoldPosition()
        {
            if (agent == null || !agent.enabled) return;

            agent.isStopped = true;
            if (agent.isOnNavMesh)
                agent.velocity = Vector3.zero;

            UpdateAnimatorImmediate(0f);
        }
    }
}
