using UnityEngine;
using UnityEngine.AI;

public class ChaseMovement : MonoBehaviour, IMinibossMovement
{
    private NavMeshAgent agent;
    [SerializeField] private Transform target;
    [SerializeField] private float repathInterval = 0.25f;

    private float repathTimer;
    private bool isMoving;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        if (!isMoving || target == null) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            agent.SetDestination(target.position);
        }
    }

    public void StartMove()
    {
        if (agent != null)
        {
            if (!agent.enabled || target == null) return;

            isMoving = true;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            repathTimer = 0f;
        }
    }

    public void StopMove()
    {
        isMoving = false;

        if (agent != null)
        {
            if (!agent.enabled) return;

            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}