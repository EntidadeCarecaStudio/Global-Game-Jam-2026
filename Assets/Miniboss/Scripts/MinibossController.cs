using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Observer;

public class MinibossController : BaseCharacterController
{
    [Header("↑ Só BaseCharacterStats está sendo usado ↑")]

    [Header("References")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("Combat Context (for debugging purposes only)")]
    [SerializeField] private CombatContext _combatContext = new CombatContext();

    private StatsBinder _stats;
    private MinibossAnimation _animator;

    private AttackSelector _attackSelector;
    private AttackExecutor _attackExecutor;

    public Transform AttackPoint => _attackPoint;
    public StatsBinder StatsBinder => _stats;
    public MinibossAnimation Animator => _animator;

    public AttackSelector AttackSelector => _attackSelector;
    public AttackExecutor AttackExecutor => _attackExecutor;

    public CombatContext CombatContext => _combatContext;
    
    public MovementContext MovementContext { get; private set; }

    public IMinibossState CurrentBossState { get; private set; }
    public IMinibossState IdleState { get; private set; }
    public IMinibossState ChaseState { get; private set; }
    public IMinibossState AttackState { get; private set; }
    public IMinibossState DieState { get; private set; }

    protected override void Awake()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        NavMeshAgent agent = GetComponent<NavMeshAgent>();

        _stats = GetComponent<StatsBinder>();
        _animator = GetComponent<MinibossAnimation>();

        _attackSelector = GetComponent<AttackSelector>();
        _attackExecutor = GetComponent<AttackExecutor>();

        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;

        MovementContext = new MovementContext
        {
            Self = transform,
            Target = player,
            Agent = agent,
            Controller = this,
            MinSafeDistance = StatsBinder.Stats.minSafeDistance,
            RepositionRadius = StatsBinder.Stats.repositionRadius
        };

        StatsBinder.ApplyStats(MovementContext);

        IdleState = new IdleState(this);
        ChaseState = new ChaseState(this);
        AttackState = new AttackState(this);
        DieState = new DieState(this);

        _combatContext.ResetAttackTimers();
    }

    protected override void Start()
    {
        CalculateEffectiveStats();
        m_currentHealth = m_effectiveStats.maxHealth;

        ChangeState(IdleState);
    }

    protected override void Update()
    {
        UpdateMovementContext();
        UpdateCombatContext();
        CurrentBossState?.Tick();
    }

    public void ChangeState(IMinibossState newState)
    {
        CurrentBossState?.Exit();
        CurrentBossState = newState;
        CurrentBossState.Enter();
    }

    private void UpdateMovementContext()
    {
        MovementContext.DeltaTime = Time.deltaTime;
    }

    private void UpdateCombatContext()
    {
        _combatContext.timeInCombat += Time.deltaTime;
        _combatContext.timeSinceLastAttack += Time.deltaTime;

        _combatContext.currentDistance = MovementContext.DistanceToTarget;

        if (_combatContext.currentDistance <= _stats.Stats.attackRange)
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
        if (eventName == "AttackEnds")
        {
            if (CurrentBossState is AttackState attackState)
                attackState.OnAttackFinished();
        }

        if (eventName == "HitDetection")
        {
            if ((_spriteRenderer.flipX ? -1f : 1f) * _attackPoint.localPosition.x < 0f)
            {
                _attackPoint.localPosition = new Vector3(
                    _attackPoint.localPosition.x * -1f,
                    _attackPoint.localPosition.y,
                    _attackPoint.localPosition.z);
            }

            PerformAttackDetection(_attackPoint, _attackRadius);
        }
    }


    public void PerformAttackDetection(Transform attackPoint, float attackRadius)
    {
        Collider[] hitPlayer = Physics.OverlapSphere(attackPoint.position, attackRadius, _playerLayer);
        foreach (Collider playerCollider in hitPlayer)
        {
            if (playerCollider.gameObject == gameObject || !playerCollider.isTrigger)
                continue;

            if (playerCollider.TryGetComponent(out IDamageable damageablePlayer))
            {
                _combatContext.ResetAttackTimers();
                damageablePlayer.TakeDamage(m_effectiveStats.attackDamage, transform.position);
            }
            else _combatContext.RegisterFailedAttack();
        }
    }

    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (CurrentBossState is DieState dieState) return;

        float effectiveDamage = damage;

        m_currentHealth -= Mathf.RoundToInt(effectiveDamage);

        UpdateHealthUI();

        if (m_currentHealth <= 0)
        {
            m_currentHealth = 0;
            ChangeState(DieState);
        }
        else
        {
            //ChangeState(StunState);

            Vector3 knockbackDirection = (transform.position - hitSourcePosition).normalized;
            Vector3 flatKnockbackDirection = new Vector3(knockbackDirection.x, 0f, knockbackDirection.z).normalized;
            
            float finalKnockbackForce = m_effectiveStats.knockbackForce * (1f - m_effectiveStats.knockbackResistance);

            if (finalKnockbackForce > 0.01f && m_rigidbody != null)
            {
                //m_rigidbody.AddForce(flatKnockbackDirection * finalKnockbackForce, ForceMode.Impulse);
                StartCoroutine(Knockback(flatKnockbackDirection, finalKnockbackForce));
            }

            if (m_damageFeedbackCoroutine != null) StopCoroutine(m_damageFeedbackCoroutine);
            m_damageFeedbackCoroutine = StartCoroutine(DamageFeedback());
        }
    }

    private IEnumerator Knockback(Vector3 knockDir, float force)
    {
        MovementContext.Agent.enabled = false;
        m_rigidbody.isKinematic = false;

        m_rigidbody.AddForce(knockDir * force, ForceMode.Impulse);

        yield return new WaitForSeconds(0.2f);

        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.isKinematic = true;
        MovementContext.Agent.enabled = true;
    }

    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = m_effectiveStats.maxHealth;
            _healthSlider.value = m_currentHealth;
        }
    }

    public void OnDie()
    {
        Die();
    }
    protected override void Die()
    {
        _animator.PlayDie();
    }
}
