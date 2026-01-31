// EnemyController.cs
using UnityEngine;
using System.Collections;

public class EnemyTestController : BaseCharacterController
{
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _detectionRange = 5.0f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _timeToDestroyAfterDeath = 3.0f;
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;

    private Transform m_playerTransform;
    private float m_attackCooldownTimer;
    private bool m_hasDealtDamageInCurrentAttack;
    private Vector3 m_facingDirection = Vector3.back;

    protected override void Start()
    {
        base.Start(); // Chama o Start da classe base

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            m_playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player GameObject not found! Enemy will not chase.");
        }
        SetState(CharacterState.Idle);
    }

    protected override void Update()
    {
        base.Update(); // Chama o Update da classe base
        UpdateStateLogic();
        UpdateSpriteOrientation(m_facingDirection); // Usa o método do base

        if (m_attackCooldownTimer > 0)
        {
            m_attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        PerformMovement();
    }

    private void UpdateStateLogic()
    {
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
            case CharacterState.Dodge:
                // Inimigos não têm Dodge por padrão neste script
                break;
            // O estado Die não precisa de um HandleDieStateLogic aqui, pois o método Die() já lida com as ações finais.
        }
    }

    private void HandleIdleStateLogic()
    {
        if (m_playerTransform == null) return;

        float distanceSqrToPlayer = (transform.position - m_playerTransform.position).sqrMagnitude;
        if (distanceSqrToPlayer <= _detectionRange * _detectionRange)
        {
            SetState(CharacterState.Run);
        }
    }

    private void HandleRunStateLogic()
    {
        if (m_playerTransform == null) return;

        float distanceSqrToPlayer = (transform.position - m_playerTransform.position).sqrMagnitude;

        if (m_attackCooldownTimer <= 0 && distanceSqrToPlayer <= _attackRange * _attackRange)
        {
            SetState(CharacterState.Attack);
        }
        else if (distanceSqrToPlayer > _detectionRange * _detectionRange)
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

        float normalizedTime = m_stateTimer / m_effectiveStats.attackDuration;

        if (normalizedTime >= _attackWindowStartPercentage && normalizedTime <= _attackWindowEndPercentage && !m_hasDealtDamageInCurrentAttack)
        {
            PerformAttackDetection();
            m_hasDealtDamageInCurrentAttack = true;
        }

        if (m_stateTimer >= m_effectiveStats.attackDuration)
        {
            m_attackCooldownTimer = m_effectiveStats.attackCooldown;
            if (m_playerTransform != null && (transform.position - m_playerTransform.position).sqrMagnitude <= _detectionRange * _detectionRange)
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
                damageablePlayer.TakeDamage(m_effectiveStats.attackDamage, _attackPoint.position);
            }
        }
    }

    private void HandleTakeDamageStateLogic()
    {
        if (m_stateTimer >= m_effectiveStats.takeDamageStunDuration)
        {
            if (m_currentHealth <= 0)
            {
                // Este caminho já foi coberto por Die() no TakeDamage base.
                // Aqui podemos simplesmente ir para Run/Idle se o inimigo não morre imediatamente.
                if (m_playerTransform != null && (transform.position - m_playerTransform.position).sqrMagnitude <= _detectionRange * _detectionRange)
                {
                    SetState(CharacterState.Run);
                }
                else
                {
                    SetState(CharacterState.Idle);
                }
            }
            else
            {
                if (m_playerTransform != null && (transform.position - m_playerTransform.position).sqrMagnitude <= _detectionRange * _detectionRange)
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

    // Implementação específica de Die() para o Inimigo
    protected override void Die()
    {
        SetState(CharacterState.Die); // Define o estado para animação, se houver
        m_rigidbody.linearVelocity = Vector3.zero; // Para movimento
        m_rigidbody.isKinematic = true; // Impede que o inimigo seja empurrado após morrer
        Collider ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false; // Desabilita colisão
        }
        enabled = false; // Desabilita o script do inimigo
        StartCoroutine(DestroyAfterDelay(_timeToDestroyAfterDeath)); // Corotina para destruir o inimigo após morrer
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void PerformMovement()
    {
        Vector3 direction = Vector3.zero;
        if (m_currentState == CharacterState.Run && m_playerTransform != null)
        {
            direction = (m_playerTransform.position - transform.position).normalized;
        }

        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = direction.x * m_effectiveStats.movementSpeedX;
        currentVelocity.z = direction.z * m_effectiveStats.movementSpeedZ;
        m_rigidbody.linearVelocity = currentVelocity;
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