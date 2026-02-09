using UnityEngine;
using UnityEngine.AI;

public struct AttackContext
{
    public Transform attacker;
    public Transform target;
    public NavMeshAgent agent;
    public AttackExecutor executor;
}
