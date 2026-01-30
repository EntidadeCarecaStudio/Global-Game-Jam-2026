using UnityEngine;

public interface IMinibossState
{
    void EnterState();
    void Tick();
    void ExitState();
}

public class IdleState : IMinibossState
{
    public void EnterState() { }
    public void Tick() { }
    public void ExitState() { }
}

public class ChaseState : IMinibossState
{
    private readonly MinibossController _controller;
    private IMinibossMovement _movement;

    private float attackCheckInterval = 0.25f;
    private float timer;

    public ChaseState(MinibossController controller, IMinibossMovement movement)
    {
        this._controller = controller;
        this._movement = movement;
    }

    public void EnterState()
    {
        timer = 0f;
        _movement.StartMove();
    }

    public void Tick()
    {

        timer += Time.deltaTime;
        if (timer < attackCheckInterval)
            return;
        timer = 0f;
        bool hasAttack = _controller.AttackSelector.SelectAttack(_controller.CombatContext) != null;

        if (hasAttack)
        {
            Debug.Log("É pra estar chamando um ataque");
            _controller.ChangeState(_controller.AttackState);
        }
    }

    public void ExitState()
    {
        _movement.StopMove();
    }
}

public class AttackState : IMinibossState
{
    private readonly MinibossController _controller;
    private SO_AttackData _currentAttack;

    public AttackState(MinibossController controller)
    {
        this._controller = controller;
    }

    public void EnterState()
    {
        Debug.Log("Entrou em AttackState");
        _currentAttack = _controller.AttackSelector.SelectAttack(_controller.CombatContext);

        if (_currentAttack == null)
        {
            _controller.CombatContext.RegisterFailedAttack();
            _controller.ChangeState(_controller.ChaseState);
            return;
        }

        _controller.CombatContext.ResetAttackTimers();

        _currentAttack.Execute(_controller.AttackContext);
        _currentAttack.StartCooldown(_controller);
    }

    public void Tick() { }
    public void ExitState() { }

    public void OnAttackFinished()
    {
        _controller.ChangeState(this);
    }
}