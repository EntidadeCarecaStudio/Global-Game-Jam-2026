using UnityEngine;
using UnityEngine.Playables;

public interface IMinibossState
{
    void EnterState();
    void ExitState();
}

public class IdleState : IMinibossState
{
    public void EnterState() { }
    public void ExitState() { }
}

public class ChaseState : IMinibossState
{
    private readonly MinibossController controller;
    private IMinibossMovement movement;

    public ChaseState(MinibossController controller, IMinibossMovement movement)
    {
        this.controller = controller;
        this.movement = movement;
    }

    public void EnterState()
    {
        movement.StartMove();
        controller.ProximitySensor.OnTargetEnterRange += HandleTargetEnterRange;
    }

    public void ExitState()
    {
        movement.StopMove();
        controller.ProximitySensor.OnTargetEnterRange -= HandleTargetEnterRange;
    }

    private void HandleTargetEnterRange(Transform target)
    {
        controller.ChangeState(controller.AttackState);
    }
}

public class AttackState : IMinibossState
{
    private readonly MinibossController controller;

    public AttackState(MinibossController controller)
    {
        this.controller = controller;
    }

    public void EnterState()
    {
        controller.PerformAttack();
    }
    public void ExitState() { }

    public void OnAttackFinished()
    {
        if (controller.ProximitySensor.IsTargetInRange)
        {
            controller.ChangeState(this);
        }
        else
        {
            controller.ChangeState(controller.ChaseState);
        }
    }
}