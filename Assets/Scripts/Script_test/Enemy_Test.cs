using UnityEngine;

public class Enemy_Test : BaseCharacterController
{
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;

    private Vector3 m_facingDirection = Vector3.back;
    private bool m_hasAttackedInCurrentWindow;

    protected override void Start()
    {
        base.Start();
        SetState(CharacterState.Idle);
    }

    protected override void Update()
    {
        base.Update();
        UpdateStateLogic();
        UpdateSpriteOrientation(m_facingDirection);
    }

    private void UpdateStateLogic()
    {
        switch (m_currentState)
        {
            case CharacterState.Idle:
                // Não faz nada, a IA controla quando atacar
                break;
            case CharacterState.Attack:
                HandleAttackStateLogic();
                break;
            case CharacterState.TakeDamage:
                HandleTakeDamageStateLogic();
                break;
            case CharacterState.Die:
                // Não faz nada, aguardando destruição ou animação
                break;
        }
    }

    private void HandleAttackStateLogic()
    {
        float normalizedTime = m_stateTimer / m_effectiveStats.attackDuration;

        if (normalizedTime >= _attackWindowStartPercentage && normalizedTime <= _attackWindowEndPercentage && !m_hasAttackedInCurrentWindow)
        {
            PerformAttackDetection();
            m_hasAttackedInCurrentWindow = true;
        }

        if (m_stateTimer >= m_effectiveStats.attackDuration)
        {
            // Volta para o estado Idle após o ataque
            SetState(CharacterState.Idle);
        }
    }

    private void PerformAttackDetection()
    {
        if (_attackPoint == null)
        {
            Debug.Log("O Inimigo está batendo");
            //Debug.LogWarning("Attack point not assigned for enemy attack detection.");
            return;
        }

        Collider[] hitPlayers = Physics.OverlapSphere(_attackPoint.position, _attackRadius, _playerLayer);
        foreach (Collider playerCollider in hitPlayers)
        {
            if (playerCollider.gameObject == gameObject || !playerCollider.isTrigger)
                continue;

            if (playerCollider.TryGetComponent(out IDamageable damageablePlayer))
            {
                damageablePlayer.TakeDamage(m_effectiveStats.attackDamage, transform.position);
            }
        }
    }

    private void HandleTakeDamageStateLogic()
    {
        if (m_stateTimer >= m_effectiveStats.takeDamageStunDuration)
        {
            // Após o tempo de stun, volta para o estado Idle
            SetState(CharacterState.Idle);
        }
    }

    protected override void Die()
    {
        SetState(CharacterState.Die);
        m_rigidbody.linearVelocity = Vector3.zero;
        enabled = false;
        Debug.Log("Enemy has died!");
    }

    // Método para a IA iniciar um ataque
    public void StartAttack()
    {
        if (m_currentState == CharacterState.Idle)
        {
            SetState(CharacterState.Attack);
            m_hasAttackedInCurrentWindow = false;
        }
    }

    // Método para a IA definir a direção de enfrentamento
    public void SetFacingDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            m_facingDirection = direction.normalized;
        }
    }

    // Sobrescrevendo o TakeDamage para que o inimigo possa receber dano
    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (m_currentState == CharacterState.Die) return;

        base.TakeDamage(damage, hitSourcePosition);

        // Se ainda estiver vivo, entra no estado de tomar dano
        if (m_currentHealth > 0)
        {
            SetState(CharacterState.TakeDamage);
        }
    }

    protected override void UpdateSpriteOrientation(Vector3 direction)
    {
        base.UpdateSpriteOrientation(direction);

        // Ajusta a posição do ponto de ataque baseado na direção
        if (_attackPoint != null)
        {
            if (direction.x > 0)
            {
                var position = _attackPoint.localPosition;
                var x = Mathf.Abs(position.x);
                position.x = x;
                _attackPoint.localPosition = position;
            }
            else if (direction.x < 0)
            {
                var position = _attackPoint.localPosition;
                var x = Mathf.Abs(position.x);
                position.x = -x;
                _attackPoint.localPosition = position;
            }
        }
    }

    // Não há mais input, então removemos os métodos OnMove, OnAttack, etc.
    // Também removemos os eventos de input.

    private void OnDrawGizmosSelected()
    {
        if (_attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_attackPoint.position, _attackRadius);
        }
    }
}