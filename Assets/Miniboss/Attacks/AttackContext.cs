using UnityEngine;
using UnityEngine.AI;

public struct AttackContext
{
    public MonoBehaviour runner;
    public Transform attacker;
    public Transform target;
    public NavMeshAgent agent;
    public MinibossAnimation animator;
    public MonoBehaviour coroutineRunner;
}
