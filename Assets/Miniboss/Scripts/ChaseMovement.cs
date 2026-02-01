using UnityEngine;
using UnityEngine.AI;

public class ChaseMovement : MonoBehaviour, IMinibossMovement
{
    [SerializeField] private float repathInterval = 0.25f;

    private NavMeshAgent _agent;

    private float repathTimer = 0f;

    private void Start()
    {
        if (TryGetComponent<MinibossController>(out MinibossController controller))
            _agent = controller.Agent;
        else
            Debug.LogError("Needs MinibossController component!");
    }

    public void StartMove()
    {
        if (_agent != null)
        {
            if (!_agent.enabled) return;
            
            _agent.isStopped = false;
            repathTimer = 0f;
        }
    }

    public void StopMove()
    {
        if (_agent != null)
        {
            if (!_agent.enabled) return;

            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    public void ChaseMove(Vector3 targetPosition)
    {
        if (!_agent.enabled) return;
        if (_agent.isStopped || targetPosition == null) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            _agent.SetDestination(targetPosition);
        }
    }

    public bool IsInRange(Vector3 targetPosition, float range)
    {
        return Vector3.Distance(_agent.transform.position, targetPosition) <= range;
    }
}