using UnityEngine;

public class MinibossController : MonoBehaviour
{
    //[SerializeField] private Transform player;
    [SerializeField] private MinibossProximitySensor proximitySensor;
    private MinibossAnimation animator;

    //public Transform Target => player;
    public MinibossProximitySensor ProximitySensor => proximitySensor;

    private IMinibossMovement movement;
    private IMinibossState currentState;

    public IMinibossState ChaseState { get; private set; }
    public IMinibossState AttackState { get; private set; }

    private void Awake()
    {
        movement = GetComponent<IMinibossMovement>();
        animator = GetComponent<MinibossAnimation>();

        ChaseState = new ChaseState(this, movement);
        AttackState = new AttackState(this);
    }

    private void Start()
    {
        ChangeState(ChaseState);
    }

    public void ChangeState(IMinibossState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }

    public void OnAnimationEvent(string eventName)
    {
        /*if (currentState is IAnimationEventListener listener)
        {
            listener.OnAnimationEvent(eventName);
        }*/
        
        if (currentState is AttackState attackState)
        {
            attackState.OnAttackFinished();
        }
    }

    public void PerformAttack()
    {
        animator.PlayAttack();
    }
}
