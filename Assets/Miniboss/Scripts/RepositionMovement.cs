using UnityEngine;
using UnityEngine.AI;

public class RepositionMovement : IMinibossMovement
{
    private Vector3 targetPosition;
    private float elapsed;
    private bool active;

    private readonly float maxDuration;
    private readonly float stopDistance;

    public bool IsFinished => !active;

    public RepositionMovement(
        float maxDuration = 0.6f,
        float stopDistance = 0.4f)
    {

        this.maxDuration = maxDuration;
        this.stopDistance = stopDistance;
    }

    public void StartMove(MovementContext context)
    {
        if (context.Agent != null)
        {
            if (!context.Agent.enabled) return;

            elapsed = 0f;
            active = true;

            targetPosition = CalculateRepositionPoint(context);

            context.Agent.isStopped = false;
            context.Agent.SetDestination(targetPosition);
        }
    }

    public void Tick(MovementContext context)
    {
        if (!active) return;

        elapsed += context.DeltaTime;

        if (HasReachedDestination(context) || elapsed >= maxDuration)
        {
            StopMove(context);
        }
    }

    public void StopMove(MovementContext context)
    {
        if (context.Agent != null)
        {
            if (!context.Agent.enabled) return;

            context.Agent.isStopped = true;
            context.Agent.ResetPath();
        }

        active = false;
    }

    private Vector3 CalculateRepositionPoint(MovementContext context)
    {
        Vector3 awayFromTarget =
            (context.Self.position - context.Target.position).normalized;

        Vector3 lateral =
            Vector3.Cross(Vector3.up, awayFromTarget).normalized;

        if (Random.value > 0.5f)
            lateral = -lateral;

        Vector3 desiredDirection =
            (awayFromTarget * 0.7f + lateral * 0.3f).normalized;

        Vector3 candidate =
            context.Self.position +
            desiredDirection * context.RepositionRadius;

        if (NavMesh.SamplePosition(
            candidate,
            out NavMeshHit hit,
            context.RepositionRadius,
            NavMesh.AllAreas))
        {

            return hit.position;
        }

        return context.Self.position + awayFromTarget * context.RepositionRadius;
    }

    private bool HasReachedDestination(MovementContext context)
    {
        if (context.Agent.enabled)
        {
            if (context.Agent.pathPending)
            return false;

            return context.Agent.remainingDistance <= stopDistance;
        }

        return true;
    }
}
