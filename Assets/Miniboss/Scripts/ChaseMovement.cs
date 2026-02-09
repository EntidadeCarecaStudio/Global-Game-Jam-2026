using UnityEngine;
using UnityEngine.AI;

public class ChaseMovement : MonoBehaviour, IMinibossMovement
{
    [SerializeField] private float repathInterval = 0.25f;

    private float repathTimer = 0f;

    public void StartMove(MovementContext context)
    {
        if (context.Agent != null)
        {
            if (!context.Agent.enabled) return;

            context.Agent.isStopped = false;
            repathTimer = 0f;
        }
    }

    public void Tick(MovementContext context)
    {
        if (!context.Agent.enabled) return;
        if (context.Agent.isStopped || context.Target.position == null) return;

        repathTimer -= Time.deltaTime;
        if (repathTimer <= 0f)
        {
            repathTimer = repathInterval;
            context.Agent.SetDestination(context.Target.position);
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
    }
}