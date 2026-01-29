using UnityEngine;

public class MinibossController : MonoBehaviour
{
    [SerializeField] private IMinibossState currentState;
    [SerializeField] private IMinibossMovement movement;

    private void Awake()
    {
        movement = GetComponent<IMinibossMovement>();
    }

    private void Start()
    {
        ChangeState(new ChaseState(movement));
    }

    public void ChangeState(IMinibossState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState.EnterState();
    }
}
