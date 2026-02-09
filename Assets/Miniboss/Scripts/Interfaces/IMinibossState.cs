using UnityEngine;

public interface IMinibossState
{
    void Enter();
    void Tick();
    void Exit();
}

public class IdleState : IMinibossState
{
    private readonly MinibossController controller;

    public IdleState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        controller.MovementContext.Agent.isStopped = true;
        controller.MovementContext.Agent.enabled = false;
        controller.Animator.PlayIdle();
    }
    public void Tick()
    {
        if (controller.MovementContext.DistanceToTarget <= controller.StatsBinder.Stats.chaseRange)
        {
            controller.ChangeState(controller.ChaseState);
        }
    }
    public void Exit() { }
}

public class ChaseState : IMinibossState
{
    private readonly MinibossController controller;

    private readonly IMinibossMovement chaseMovement;
    private readonly RepositionMovement repositionMovement;

    private IMinibossMovement currentMovement;

    private const float RepositionCooldown = 2f;
    private float repositionTimer;

    private float attackCheckInterval = 0.25f;
    private float attckTimer;

    public ChaseState(MinibossController controller)
    {
        this.controller = controller;

        chaseMovement = new ChaseMovement();
        repositionMovement = new RepositionMovement();
    }

    public void Enter()
    {
        repositionTimer = 0f;
        attckTimer = 0f;

        controller.MovementContext.Agent.enabled = true;

        SetMovement(chaseMovement);
    }

    public void Tick()
    {
        var context = controller.MovementContext;

        repositionTimer -= context.DeltaTime;

        if (ShouldReposition(context))
        {
            SetMovement(repositionMovement);
        }

        currentMovement.Tick(context);

        if (currentMovement == repositionMovement &&
            repositionMovement.IsFinished)
        {
            repositionTimer = RepositionCooldown;
            SetMovement(chaseMovement);
        }

        attckTimer += Time.deltaTime;
        if (attckTimer < attackCheckInterval) return;
        attckTimer = 0f;

        bool hasAttack = controller.AttackSelector.SelectAttack(controller.CombatContext, controller.AttackExecutor) != null;

        if (hasAttack)
            controller.ChangeState(controller.AttackState);
    }

    public void Exit()
    {
        currentMovement?.StopMove(controller.MovementContext);
    }

    private bool ShouldReposition(MovementContext context)
    {
        return context.DistanceToTarget < context.MinSafeDistance &&
               repositionTimer <= 0f &&
               currentMovement != repositionMovement;
    }

    private void SetMovement(IMinibossMovement movement)
    {
        currentMovement?.StopMove(controller.MovementContext);
        currentMovement = movement;
        currentMovement.StartMove(controller.MovementContext);

        controller.Animator.PlayRun();
    }
}

public class AttackState : IMinibossState
{
    private readonly MinibossController controller;

    public AttackState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        TryAttack();
    }

    public void Tick()
    {
        if (!controller.AttackExecutor.IsBusy)
        {
            controller.ChangeState(controller.ChaseState);
        }
    }
    public void Exit() { }

    public void TryAttack()
    {
        var currentAttack = controller.AttackSelector.SelectAttack(controller.CombatContext, controller.AttackExecutor);

        if (currentAttack == null)
        {
            controller.CombatContext.RegisterFailedAttack();
            controller.ChangeState(controller.ChaseState);
            return;
        }

        AttackContext context = new AttackContext
        {
            attacker = controller.transform,
            target = controller.MovementContext.Target,
            agent = controller.MovementContext.Agent,
            executor = controller.AttackExecutor
        };

        bool executed = controller.AttackExecutor.ExecuteAttack(currentAttack, context);
        if (executed)
            controller.Animator.PlayAttack(currentAttack);
    }

    public void OnAttackFinished()
    {
        controller.ChangeState(this);
    }
}

public class StunState : IMinibossState
{
    private readonly MinibossController controller;

    private float stunTimer;

    public StunState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        stunTimer = 0f;
        controller.Animator.PlayHit();
    }
    public void Tick()
    {
        stunTimer += Time.deltaTime;
        if (stunTimer < controller.StatsBinder.CStats.takeDamageStunDuration) return;
        stunTimer = 0f;

        controller.ChangeState(controller.ChaseState);
    }
    public void Exit() { }
}

public class DieState : IMinibossState
{
    private readonly MinibossController controller;

    public DieState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        controller.OnDie();
    }
    public void Tick() { }
    public void Exit() { }
}