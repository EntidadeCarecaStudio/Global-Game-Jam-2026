using UnityEngine;
using UnityEngine.AI;

public class StatsBinder : MonoBehaviour
{
    [SerializeField] private SO_MinibossStats _stats;
    [SerializeField] private CharacterStats _cStats;

    private NavMeshAgent _agent;

    public SO_MinibossStats Stats => _stats;
    public CharacterStats CStats => _cStats;

    private void Start()
    {
        if (TryGetComponent<MinibossController>(out MinibossController controller))
            _agent = controller.Agent;
        else
            Debug.LogError("Needs MinibossController component!");

        ApplyStats();
    }

    private void ApplyStats()
    {
        if (_agent != null && _stats != null)
        {
            _agent.speed = _cStats.movementSpeedX; //_stats.moveSpeed;
            _agent.acceleration = _stats.moveAcceleration;
            _agent.angularSpeed = 0f;
            _agent.stoppingDistance = _stats.stopDistance;
        }
    }
}
