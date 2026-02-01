using UnityEngine;
using UnityEngine.UI; // Necessário para a Barra de Vida
using System.Collections;

public class EnemyController : BaseCharacterController
{
    [Header("UI Settings")]
    [SerializeField] private Slider _healthSlider; // Arraste o Slider aqui
    [SerializeField] private GameObject _uiCanvas; // Opcional: Canvas para sumir ao morrer

    [Header("AI Settings")]
    [SerializeField] private float _detectionRange = 5.0f;
    [SerializeField] private float _attackRange = 1.5f; // Distância para começar a atacar
    [SerializeField] private float _damageReach = 2.0f; // Distância máxima para o hit conectar (geralmente um pouco maior que o AttackRange)
    [SerializeField] private float _timeToDestroyAfterDeath = 3.0f;

    [Header("Animation Settings")]
    // Define em qual momento da animação o dano é aplicado (ex: entre 30% e 80%)
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;

    private Transform _targetPlayer;
    private float m_attackCooldownTimer;
    private bool m_hasDealtDamageInCurrentAttack;
    private Vector3 m_facingDirection = Vector3.back;

    protected override void Start()
    {
        base.Start();

        // Configura UI Inicial
        UpdateHealthUI();

        // Tenta achar o player automaticamente (Método do script simples que funcionou)
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            _targetPlayer = player.transform;
        }
    }

    protected override void Update()
    {
        base.Update(); // Atualiza m_stateTimer
        UpdateStateLogic();
        UpdateSpriteOrientation(m_facingDirection);

        // Cooldown entre ataques
        if (m_attackCooldownTimer > 0)
        {
            m_attackCooldownTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        PerformMovement();
    }

    // --- LÓGICA DE UI E DANO RECEBIDO ---
    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        base.TakeDamage(damage, hitSourcePosition); // Aplica dano, knockback, cor vermelha
        UpdateHealthUI(); // Atualiza a barra
    }

    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = m_effectiveStats.maxHealth;
            _healthSlider.value = m_currentHealth;
        }
    }
    // ------------------------------------

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
        }
    }

    private void HandleIdleStateLogic()
    {
        if (_targetPlayer == null) return;

        float distance = Vector3.Distance(transform.position, _targetPlayer.position);
        if (distance <= _detectionRange)
        {
            SetState(CharacterState.Run);
        }
    }

    private void HandleRunStateLogic()
    {
        if (_targetPlayer == null) return;

        float distance = Vector3.Distance(transform.position, _targetPlayer.position);

        if (m_attackCooldownTimer <= 0 && distance <= _attackRange)
        {
            SetState(CharacterState.Attack);
        }
        else if (distance > _detectionRange)
        {
            SetState(CharacterState.Idle);
        }
        else
        {
            // Define direção do sprite
            Vector3 dir = (_targetPlayer.position - transform.position).normalized;
            if (dir.x != 0) m_facingDirection = new Vector3(dir.x, 0, 0).normalized;

            PerformMovement();
        }
    }

    private void HandleAttackStateLogic()
    {
        // Para o movimento durante o ataque
        m_rigidbody.linearVelocity = Vector3.zero;

        // Calcula porcentagem da animação
        float normalizedTime = m_stateTimer / m_effectiveStats.attackDuration;

        // JANELA DE DANO:
        // Verifica se está no momento certo E se ainda não deu dano neste ataque
        if (normalizedTime >= _attackWindowStartPercentage &&
            normalizedTime <= _attackWindowEndPercentage &&
            !m_hasDealtDamageInCurrentAttack)
        {
            TryApplyDamage();
            m_hasDealtDamageInCurrentAttack = true; // Garante que só bate 1 vez por animação
        }

        // Fim do ataque
        if (m_stateTimer >= m_effectiveStats.attackDuration)
        {
            m_attackCooldownTimer = m_effectiveStats.attackCooldown;
            m_hasDealtDamageInCurrentAttack = false; // Reseta para o próximo ataque

            // Decide se volta a correr ou fica parado
            if (_targetPlayer != null && Vector3.Distance(transform.position, _targetPlayer.position) <= _detectionRange)
                SetState(CharacterState.Run);
            else
                SetState(CharacterState.Idle);
        }
    }

    // --- AQUI ESTÁ A CORREÇÃO PRINCIPAL ---
    private void TryApplyDamage()
    {
        if (_targetPlayer == null) return;

        // Em vez de usar Physics.OverlapSphere (que falhou), usamos Distância Simples
        // Mas fazemos isso DENTRO da janela de tempo da animação.
        float distance = Vector3.Distance(transform.position, _targetPlayer.position);

        // Se o player ainda estiver perto o suficiente (dentro do alcance do golpe)
        if (distance <= _damageReach)
        {
            var damageable = _targetPlayer.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(m_effectiveStats.attackDamage, transform.position);
                // Debug.Log("Hit confirmadado via Distância!");
            }
        }
    }
    // --------------------------------------

    private void HandleTakeDamageStateLogic()
    {
        if (m_stateTimer >= m_effectiveStats.takeDamageStunDuration)
        {
            if (_targetPlayer != null && Vector3.Distance(transform.position, _targetPlayer.position) <= _detectionRange)
                SetState(CharacterState.Run);
            else
                SetState(CharacterState.Idle);
        }
    }

    protected override void Die()
    {
        SetState(CharacterState.Die);
        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.isKinematic = true;

        if (TryGetComponent(out Collider col)) col.enabled = false;
        if (_uiCanvas != null) _uiCanvas.SetActive(false);

        enabled = false;
        StartCoroutine(DestroyAfterDelay(_timeToDestroyAfterDeath));
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void PerformMovement()
    {
        Vector3 direction = Vector3.zero;
        if (m_currentState == CharacterState.Run && _targetPlayer != null)
        {
            direction = (_targetPlayer.position - transform.position).normalized;
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

        // Mostra o alcance do Dano
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, _damageReach);
    }
}