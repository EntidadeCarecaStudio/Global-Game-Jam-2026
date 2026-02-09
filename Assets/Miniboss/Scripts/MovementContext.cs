using UnityEngine;
using UnityEngine.AI;

public class MovementContext
{
    public Transform Self;
    public Transform Target;
    public NavMeshAgent Agent;
    public MinibossController Controller;

    public float MinSafeDistance;
    public float RepositionRadius;
    public float DeltaTime;

    public float DistanceToTarget => Vector3.Distance(Agent.transform.position, Target.position);
}
