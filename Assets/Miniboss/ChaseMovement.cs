using UnityEngine;
using UnityEngine.AI;

public class ChaseMovement : MonoBehaviour, IMinibossMovement
{
    private NavMeshAgent _agent;
    private Transform _target;
    [SerializeField] private float repathInterval = 0.25f;

    private float repathTimer;
    private bool isMoving;

    private void Start()
    {
        if (TryGetComponent<MinibossController>(out MinibossController controller))
        {
            if (_agent == null)
                _agent = controller.Agent;

            _target = controller.Target;
        }
    }

    private void Update()
    {
        if (!isMoving || _target == null) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            _agent.SetDestination(_target.position);
        }
    }

    public void StartMove()
    {
        if (_agent != null)
        {
            if (!_agent.enabled || _target == null) return;
            
            isMoving = true;
            _agent.isStopped = false;
            _agent.SetDestination(_target.position);
            repathTimer = 0f;
        }
    }

    public void StopMove()
    {
        isMoving = false;

        if (_agent != null)
        {
            if (!_agent.enabled) return;

            _agent.isStopped = true;
            _agent.ResetPath();
        }
    }

    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }
}