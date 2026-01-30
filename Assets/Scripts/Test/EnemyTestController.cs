using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyTestController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStats _characterStats;
    [SerializeField] private CharacterAnimationController _characterAnimationController;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _detectionRange = 5.0f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _timeToDestroyAfterDeath = 3.0f;
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;

    private int m_currentHealth;
    private CharacterState m_currentState;
    private Rigidbody m_rigidbody;
    private Transform m_playerTransform;
    private float m_stateTimer;
    private float m_attackCooldownTimer;
    private bool m_hasDealtDamageInCurrentAttack;
    private Vector3 m_facingDirection = Vector3.back;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        if (_characterStats == null)
        {
            Debug.LogError("CharacterStats ScriptableObject not assigned to EnemyController.");
            enabled = false;
            return;
        }
        m_currentHealth = _characterStats.maxHealth;
        SetState(CharacterState.Idle);

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            m_playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player GameObject not found! Enemy will not chase.");
        }
    }

    private void Update()
    {
        UpdateStateLogic();
        UpdateSpriteOrientation();
        if (m_attackCooldownTimer > 0)
        {
            m_attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        PerformMovement();
    }

    private void SetState(CharacterState newState)
    {
        if (m_currentState != newState)
        {
            m_currentState = newState;
            m_stateTimer = 0f;
            m_hasDealtDamageInCurrentAttack = false;
            if (_characterAnimationController != null)
            {
                _characterAnimationController.UpdateAnimation(m_currentState);
            }
        }
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
        if (m_playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, m_playerTransform.position);
        if (distanceToPlayer <= _detectionRange)
        {
            SetState(CharacterState.Run);
        }
    }

    private void HandleRunStateLogic()
    {
        if (m_playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, m_playerTransform.position);

        if (distanceToPlayer <= _attackRange && m_attackCooldownTimer <= 0)
        {
            SetState(CharacterState.Attack);
        }
        else if (distanceToPlayer > _detectionRange)
        {
            SetState(CharacterState.Idle);
        }
        else
        {
            Vector3 directionToPlayer = (m_playerTransform.position - transform.position).normalized;
            Vector3 movementDirection = new Vector3(directionToPlayer.x, 0f, directionToPlayer.z).normalized;
            
            if (movementDirection.x != 0)
            {
                m_facingDirection = new Vector3(movementDirection.x, 0f, 0f).normalized;
            }

            PerformMovement();
        }
    }

    private void HandleAttackStateLogic()
    {
        m_rigidbody.linearVelocity = Vector3.zero;

        float normalizedTime = m_stateTimer / _characterStats.attackDuration;

        if (normalizedTime >= _attackWindowStartPercentage && normalizedTime <= _attackWindowEndPercentage && !m_hasDealtDamageInCurrentAttack)
        {
            PerformAttackDetection();
            m_hasDealtDamageInCurrentAttack = true;
        }

        if (m_stateTimer >= _characterStats.attackDuration)
        {
            m_attackCooldownTimer = _characterStats.attackCooldown;
            if (m_playerTransform != null && Vector3.Distance(transform.position, m_playerTransform.position) <= _detectionRange)
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
            Debug.LogWarning("Attack point not assigned for enemy attack detection.");
            return;
        }

        Collider[] hitPlayers = Physics.OverlapSphere(_attackPoint.position, _attackRadius, _playerLayer);
        foreach (Collider playerCollider in hitPlayers)
        {
            if (playerCollider.gameObject == gameObject)
            {
                continue;
            }

            if (playerCollider.TryGetComponent(out IDamageable damageablePlayer))
            {
                damageablePlayer.TakeDamage(_characterStats.attackDamage);
            }
        }
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
                if (m_playerTransform != null && Vector3.Distance(transform.position, m_playerTransform.position) <= _detectionRange)
                {
                    SetState(CharacterState.Run);
                }
                else
                {
                    SetState(CharacterState.Idle);
                }
            }
        }
    }

    private void HandleDieStateLogic()
    {
        if (m_stateTimer >= _timeToDestroyAfterDeath)
        {
            Destroy(gameObject);
        }
    }

    private void PerformMovement()
    {
        Vector3 direction = Vector3.zero;
        if (m_currentState == CharacterState.Run && m_playerTransform != null)
        {
            direction = (m_playerTransform.position - transform.position).normalized;
        }

        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = direction.x * _characterStats.movementSpeedX;
        currentVelocity.z = direction.z * _characterStats.movementSpeedZ;
        m_rigidbody.linearVelocity = currentVelocity;
    }

    public void TakeDamage(int damage)
    {
        if (m_currentState == CharacterState.Die || m_currentState == CharacterState.TakeDamage) return;

        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            m_currentHealth = 0;
            Die();
        }
        else
        {
            SetState(CharacterState.TakeDamage);
        }
    }

    private void Die()
    {
        SetState(CharacterState.Die);
        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.isKinematic = true;
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
        if (_attackPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_attackPoint.position, _attackRadius);
        }
    }
}