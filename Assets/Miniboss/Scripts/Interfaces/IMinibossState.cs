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
        controller.Agent.isStopped = true;
        controller.Animator.PlayIdle();
    }
    public void Tick()
    {
        if (controller.DistanceToPlayer() <= controller.StatsBinder.Stats.chaseRange)
        {
            controller.ChangeState(controller.ChaseState);
        }
    }
    public void Exit() { }
}

public class ChaseState : IMinibossState
{
    private readonly MinibossController controller;

    private float attackCheckInterval = 0.25f;
    private float timer;

    public ChaseState(MinibossController controller, IMinibossMovement movement)
    {
        this.controller = controller;
    }

    public void Enter()
    {
        timer = 0f;
        controller.Move.StartMove();
        controller.Animator.PlayRun();
    }

    public void Tick()
    {
        controller.Move.ChaseMove(controller.Target.position);

        timer += Time.deltaTime;
        if (timer < attackCheckInterval) return;
        timer = 0f;

        bool hasAttack = controller.AttackSelector.SelectAttack(controller.CombatContext, controller.AttackExecutor) != null;

        if (hasAttack)
            controller.ChangeState(controller.AttackState);
    }

    public void Exit()
    {
        controller.Move.StopMove();
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
            target = controller.Target,
            agent = controller.Agent,
            executor = controller.AttackExecutor
        };

        //controller.CombatContext.ResetAttackTimers();

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

    public StunState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void Enter() { }
    public void Tick() { }
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