using UnityEngine;
using UnityEngine.AI;

public class TrafficCar : MonoBehaviour
{
    public Transform[] waypoints;
    private int currentPointIndex = 0;
    private NavMeshAgent agent;
    private bool daChamDat = false; // Biến kiểm tra xem xe đã đáp đất chưa

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Bỏ lệnh bắt xe chạy ngay lúc này đi
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        // 1. Nếu xe chưa chạm lưới NavMesh -> Không làm gì cả, cứ đợi.
        if (!agent.isOnNavMesh)
        {
            return;
        }

        // 2. Ngay khi vừa chạm đất thành công -> Ra lệnh chạy tới điểm đầu tiên
        if (!daChamDat)
        {
            agent.SetDestination(waypoints[currentPointIndex].position);
            daChamDat = true; // Đánh dấu là đã bắt đầu chạy
        }

        // 3. Xử lý việc chuyển điểm khi đến gần đích
        if (!agent.pathPending && agent.remainingDistance < 5f)
        {
            currentPointIndex++;

            if (currentPointIndex >= waypoints.Length)
            {
                currentPointIndex = 0;
            }

            agent.SetDestination(waypoints[currentPointIndex].position);
        }
    }
}