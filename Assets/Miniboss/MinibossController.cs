using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

public class MinibossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private NavMeshAgent _agent;

    [Header("Combat")]
    [SerializeField] private SO_MinibossStats _stats;
    [SerializeField] private List<SO_AttackData> _attacks;

    [Header("Combat Context")]
    [SerializeField] private CombatContext _combatContext = new CombatContext();

    private MinibossAnimation _animator;
    private IMinibossMovement _movement;

    private AttackContext _attackContext;
    private IMinibossState _currentState;

    public Transform Target => _player;
    public NavMeshAgent Agent => _agent;
    public SO_MinibossStats Stats => _stats;
    public MinibossAnimation Animator => _animator;
    public AttackContext AttackContext => _attackContext;
    public CombatContext CombatContext => _combatContext;

    public AttackSelector AttackSelector { get; private set; }
    public IMinibossState ChaseState { get; private set; }
    public IMinibossState AttackState { get; private set; }

    private void Awake()
    {
        if(_agent == null)
        {
            if (TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
            {
                _agent = agent;
            }
        }

        _animator = GetComponent<MinibossAnimation>();
        _movement = GetComponent<IMinibossMovement>();

        _attackContext = new AttackContext
        {
            runner = this,
            attacker = transform,
            target = _player,
            agent = _agent,
            animator = _animator,
            coroutineRunner = this
        };

        AttackSelector = new AttackSelector(_attacks);

        ChaseState = new ChaseState(this, _movement);
        AttackState = new AttackState(this);
    }

    private void Start()
    {
        ChangeState(ChaseState);
    }

    private void Update()
    {
        UpdateCombatContext();
        _currentState?.Tick();
    }

    public void ChangeState(IMinibossState newState)
    {
        _currentState?.ExitState();
        _currentState = newState;
        _currentState.EnterState();
    }

    private void UpdateCombatContext()
    {

        _combatContext.timeInCombat += Time.deltaTime;
        _combatContext.timeSinceLastAttack += Time.deltaTime;

        _combatContext.currentDistance = Vector3.Distance( transform.position, Target.position );

        if (_combatContext.currentDistance <= _stats.attackEnterRange)
        {
            _combatContext.timeSincePlayerInRange += Time.deltaTime;
        }
        else
        {
            _combatContext.timeSincePlayerInRange = 0f;
        }
    }

    public void OnAnimationEvent(string eventName)
    {
        if (_currentState is AttackState attackState)
        {
            attackState.OnAttackFinished();
        }
    }
}
