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
    private IMinibossMovement movement;

    public ChaseState(IMinibossMovement movement)
    {
        this.movement = movement;
    }

    public void EnterState()
    {
        movement.StartMove();
    }
    public void ExitState()
    {
        movement.StopMove();
    }
}

public class AttackingState : IMinibossState
{
    public void EnterState() { }
    public void ExitState() { }
}