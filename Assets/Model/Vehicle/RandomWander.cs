using UnityEngine;
using UnityEngine.AI;

public class RandomWander : MonoBehaviour
{
    public float wanderRadius = 100f; // Quãng đường xe nhìn xa để chọn điểm đến
    public float wanderTimer = 10f;   // Thời gian tối đa trước khi đổi hướng

    private NavMeshAgent agent;
    private float timer;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Nếu xe đã đến đích, hoặc bị kẹt quá thời gian -> Tìm điểm mới để đi tiếp
        if (timer >= wanderTimer || (!agent.pathPending && agent.remainingDistance < 1f))
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, agent.areaMask);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    // Hàm toán học giúp tìm 1 điểm ngẫu nhiên nằm trên phần lưới màu Hồng (Road)
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }
}