using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IDamageable
{

    [SerializeField] private CharacterStats _characterStats;
    [SerializeField] private float _dodgeSpeedMultiplier = 2.0f;
    [SerializeField] private AnimationCurve _dodgeCurve;
    [SerializeField] private CharacterAnimationController _characterAnimationController;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;

    private int m_currentHealth;
    private CharacterState m_currentState;
    private Vector3 m_movementInput;
    private Vector3 m_dodgeDirection;
    private float m_stateTimer;
    private bool m_canPerformAction = true;
    private Rigidbody m_rigidbody;
    private Vector3 m_facingDirection = Vector3.back;
    private bool m_hasAttackedInCurrentWindow;

    public int CurrentHealth => m_currentHealth;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        if (_characterStats == null)
        {
            Debug.LogError("CharacterStats ScriptableObject not assigned to PlayerController.");
            enabled = false;
            return;
        }
        m_currentHealth = _characterStats.maxHealth;
        SetState(CharacterState.Idle);
    }

    private void Update()
    {
        HandleInput();
        UpdateStateLogic();
        UpdateSpriteOrientation();
    }

    private void FixedUpdate()
    {
        PerformMovement(m_movementInput);
    }

    private void SetState(CharacterState newState)
    {
        if (m_currentState != newState)
        {
            m_currentState = newState;
            m_stateTimer = 0f;
            m_hasAttackedInCurrentWindow = false;
            if (_characterAnimationController != null)
            {
                _characterAnimationController.UpdateAnimation(m_currentState);
            }
        }
    }

    private void HandleInput()
    {

    }

    private void UpdateStateLogic()
    {
        m_stateTimer += Time.deltaTime;

        switch (m_currentState)
        {
            case CharacterState.Idle:
                HandleIdleStateLogic();
                break;
            case CharacterState.Run:
                HandleRunStateLogic();
                break;
            case CharacterState.Attack:
                HandleAttackStateLogic();
                break;
            case CharacterState.Dodge:
                HandleDodgeStateLogic();
                break;
            case CharacterState.TakeDamage:
                HandleTakeDamageStateLogic();
                break;
            case CharacterState.Die:
                HandleDieStateLogic();
                break;
        }
    }

    private void HandleIdleStateLogic()
    {
        if (m_movementInput != Vector3.zero)
        {
            SetState(CharacterState.Run);
        }
    }

    private void HandleRunStateLogic()
    {
        if (m_movementInput == Vector3.zero)
        {
            SetState(CharacterState.Idle);
        }
    }

    private void HandleAttackStateLogic()
    {
        float normalizedTime = m_stateTimer / _characterStats.attackDuration;

        if (normalizedTime >= _attackWindowStartPercentage && normalizedTime <= _attackWindowEndPercentage && !m_hasAttackedInCurrentWindow)
        {
            PerformAttackDetection();
            m_hasAttackedInCurrentWindow = true;
        }

        if (m_stateTimer >= _characterStats.attackDuration)
        {
            m_canPerformAction = true;
            if (m_movementInput != Vector3.zero)
            {
                SetState(CharacterState.Run);
            }
            else
            {
                SetState(CharacterState.Idle);
            }
        }
    }

    private void PerformAttackDetection()
    {
        if (_attackPoint == null)
        {
            Debug.LogWarning("Attack point not assigned for player attack detection.");
            return;
        }

        Collider[] hitEnemies = Physics.OverlapSphere(_attackPoint.position, _attackRadius, _enemyLayer);
        foreach (Collider enemyCollider in hitEnemies)
        {
            if (!enemyCollider.isTrigger || enemyCollider.gameObject == gameObject)
                continue;

            if (enemyCollider.TryGetComponent(out IDamageable damageableEnemy))
            {
                damageableEnemy.TakeDamage(_characterStats.attackDamage);
            }
        }
    }

    private void HandleDodgeStateLogic()
    {
        if (m_stateTimer >= _characterStats.dodgeDuration)
        {
            m_canPerformAction = true;
            if (m_movementInput != Vector3.zero)
            {
                SetState(CharacterState.Run);
            }
            else
            {
                SetState(CharacterState.Idle);
            }
        }
        PerformDodgeMovement();
    }

    private void HandleTakeDamageStateLogic()
    {
        if (m_stateTimer >= 0.3f)
        {
            if (m_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                SetState(CharacterState.Idle);
            }
        }
    }

    private void HandleDieStateLogic()
    {

    }

    private void PerformMovement(Vector3 direction)
    {
        if (m_currentState == CharacterState.Dodge)
        {
            return;
        }
        if (m_currentState == CharacterState.Attack ||
            m_currentState == CharacterState.TakeDamage ||
            m_currentState == CharacterState.Die)
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = direction.x * _characterStats.movementSpeedX;
        currentVelocity.z = direction.z * _characterStats.movementSpeedZ;
        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void PerformDodgeMovement()
    {
        float curveFactor = _dodgeCurve.Evaluate(m_stateTimer / _characterStats.dodgeDuration);

        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = m_dodgeDirection.x * _characterStats.movementSpeedX * _dodgeSpeedMultiplier * curveFactor;
        currentVelocity.z = m_dodgeDirection.z * _characterStats.movementSpeedZ * _dodgeSpeedMultiplier * curveFactor;
        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void Attack()
    {
        if (m_currentState == CharacterState.Attack || m_currentState == CharacterState.Dodge ||
            m_currentState == CharacterState.TakeDamage || m_currentState == CharacterState.Die) return;

        SetState(CharacterState.Attack);
        m_canPerformAction = false;
    }

    private void Dodge()
    {
        if (m_currentState == CharacterState.Attack || m_currentState == CharacterState.Dodge ||
            m_currentState == CharacterState.TakeDamage || m_currentState == CharacterState.Die) return;

        SetState(CharacterState.Dodge);
        m_canPerformAction = false;

        if (m_movementInput != Vector3.zero)
        {
            m_dodgeDirection = m_movementInput;
        }
        else
        {
            m_dodgeDirection = m_facingDirection;
        }
    }

    public void TakeDamage(int damage)
    {
        if (m_currentState == CharacterState.Die) return;
        if (m_currentState == CharacterState.Dodge) return;

        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            m_currentHealth = 0;
            Die();
        }
        else
        {
            SetState(CharacterState.TakeDamage);
            StartCoroutine(DamageFeedback());
        }
    }

    private void Die()
    {
        SetState(CharacterState.Die);
        Debug.Log("Player has died!");
        enabled = false;
    }

    private void UpdateSpriteOrientation()
    {
        if (_spriteRenderer == null) return;

        if (m_facingDirection.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (m_facingDirection.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
    }

    private IEnumerator DamageFeedback()
    {
        if (_spriteRenderer != null)
        {
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = originalColor;
        }
    }

    private void OnMove(Vector2 value)
    {
        if (m_currentState == CharacterState.Die)
        {
            m_movementInput = Vector3.zero;
            return;
        }

        m_movementInput = new Vector3(value.x, 0f, value.y).normalized;

        if (m_movementInput.x != 0)
            m_facingDirection = new Vector3(m_movementInput.x, 0f, 0f).normalized;
    }

    private void OnAttack()
    {
        if (m_currentState == CharacterState.Die)
            return;

        if (m_canPerformAction)
            Attack();
    }

    private void OnDodge()
    {
        if (m_currentState == CharacterState.Die)
            return;

        if (m_canPerformAction)
            Dodge();
    }

    void OnEnable()
    {
        Manager_Events.Input.OnMove += OnMove;
        Manager_Events.Input.OnAttack += OnAttack;
        Manager_Events.Input.OnDodge += OnDodge;
    }

    void OnDisable()
    {
        Manager_Events.Input.OnMove -= OnMove;
        Manager_Events.Input.OnAttack -= OnAttack;
        Manager_Events.Input.OnDodge -= OnDodge;
    }

}