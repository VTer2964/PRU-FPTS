using UnityEngine;
using UnityEngine.AI;

public class NPCWaypoint : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;
    private bool daChamDat = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        // Chờ đáp xuống vỉa hè
        if (!agent.isOnNavMesh) return;

        // Bắt đầu đi tới điểm đầu tiên
        if (!daChamDat)
        {
            agent.SetDestination(waypoints[currentPointIndex].position);
            daChamDat = true;
        }

        // Khi cách đích dưới 0.5 mét thì chuyển sang điểm tiếp theo
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPointIndex++;

            if (currentPointIndex >= waypoints.Length)
            {
                currentPointIndex = 0; // Đi hết vòng thì quay lại điểm đầu
            }

            agent.SetDestination(waypoints[currentPointIndex].position);
        }
    }
}