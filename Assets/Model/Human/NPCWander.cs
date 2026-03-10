using UnityEngine;
using UnityEngine.AI;

public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 15f; // Đi lang thang trong vòng 15 mét
    public float wanderTimer = 6f;   // 6 giây sẽ tìm điểm mới một lần

    private NavMeshAgent agent;
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        // Phải chờ chân chạm đất (chạm NavMesh) mới bắt đầu chạy
        if (!agent.isOnNavMesh) return;

        timer += Time.deltaTime;

        // Nếu đã đến đích hoặc đứng quá lâu -> Tìm điểm mới
        if (timer >= wanderTimer || (!agent.pathPending && agent.remainingDistance < 0.5f))
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, agent.areaMask);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    // Hàm toán học tự tìm 1 điểm ngẫu nhiên trên vỉa hè
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;

        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }
}