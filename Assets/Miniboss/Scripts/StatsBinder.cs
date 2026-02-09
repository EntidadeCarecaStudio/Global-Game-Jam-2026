using UnityEngine;
using UnityEngine.AI;

public class StatsBinder : MonoBehaviour
{
    [SerializeField] private SO_MinibossStats _stats;
    [SerializeField] private CharacterStats _cStats;

    public SO_MinibossStats Stats => _stats;
    public CharacterStats CStats => _cStats;

    public void ApplyStats(MovementContext context)
    {
        NavMeshAgent _agent = context.Agent;

        if (_agent != null && _stats != null)
        {
            _agent.speed = _cStats.movementSpeedX; //_stats.moveSpeed;
            _agent.acceleration = _stats.moveAcceleration;
            _agent.angularSpeed = 0f;
            _agent.stoppingDistance = _stats.stopDistance;
        }
    }
}
