using UnityEngine;
using UnityEngine.AI;
using static Observer;

public class StatsBinder : MonoBehaviour
{
    [SerializeField] private SO_MinibossStats stats;
    private NavMeshAgent agent;

    public float CurrentMoveSpeed => agent.speed;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }

        ApplyStats();
    }

    private void ApplyStats()
    {
        if (agent != null && stats != null)
        {
            agent.speed = stats.moveSpeed;
            agent.acceleration = stats.moveAcceleration;
            agent.angularSpeed = 0f;
            agent.stoppingDistance = 1f;
            //agent.avoidancePriority = 50;
        }
    }

    public void BuffMoveSpeed(float multiplier)
    {
        if (agent != null && stats != null)
        {
            agent.speed = stats.moveSpeed * multiplier;
        }
    }
}
