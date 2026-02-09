using UnityEngine;
using Unity.Cinemachine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : BaseCharacterController
{
    [SerializeField] private float _dodgeSpeedMultiplier = 2.0f;
    [SerializeField] private AnimationCurve _dodgeCurve;
    [SerializeField] private Transform _attackPoint;
    [SerializeField] private float _attackRadius = 0.5f;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private float _attackWindowStartPercentage = 0.3f;
    [SerializeField] private float _attackWindowEndPercentage = 0.8f;
    

    [Header("Interaction Settings")]
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private float _interactionRange = 2.0f;
    [SerializeField] private float _interactionCheckInterval = 0.1f;

    [Header("UI Settings")]
    [SerializeField] private Slider _healthSlider;

    [Header("Cinemachine Impulse")]
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    private Vector3 m_movementInput;
    private Vector3 m_dodgeDirection;
    private bool m_canPerformAction = true;
    private Vector3 m_facingDirection = Vector3.back;
    private bool m_hasAttackedInCurrentWindow;

    private IInteractable m_currentHighlightedInteractable;
    private float m_interactionCheckTimer;


    // Eventos de som
    public static event Action OnPlayerDodge;
    public static event Action OnPlayerAttack;
    public static event Action OnPlayerGetHit;
    public static event Action OnPlayerGetKilled;

    [Header("Rotation Settings")]
    [SerializeField] private float _rotationDuration = 0.25f; // Tempo para girar 90 graus
    private bool _isRotating = false; // Trava para não spamar o botão

    [Header("Death Settings")]
    [SerializeField] private float _deathAnimationDuration = 1.5f; // Ajuste no Inspector conforme sua animação
    [SerializeField] private float _restartDelay = 2.0f; // Tempo de espera após a animação

    protected override void Awake()
    {
        base.Awake();
        if (_impulseSource == null)
        {
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            if (_impulseSource == null)
            {
                Debug.LogWarning("CinemachineImpulseSource not found on PlayerController or its GameObject. Camera shake on damage will not work.");
            }
        }
    }

    protected override void Start()
    {
        base.Start();
        UpdateHealthUI();
        SetState(CharacterState.Idle);
    }

    protected override void Update()
    {
        base.Update();
        UpdateStateLogic();
        UpdateSpriteOrientation(m_facingDirection);
        UpdateInteractableHighlight();
    }

    private void FixedUpdate()
    {
        PerformMovement(m_movementInput);
    }

    protected new void SetState(CharacterState newState)
    {
        if (m_currentState != newState)
        {
            m_currentState = newState;
            m_stateTimer = 0f;
            m_hasAttackedInCurrentWindow = false;
            
            m_canPerformAction = (newState == CharacterState.Idle || newState == CharacterState.Run);

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
        float normalizedTime = m_stateTimer / m_effectiveStats.attackDuration;

        if (normalizedTime >= _attackWindowStartPercentage && normalizedTime <= _attackWindowEndPercentage && !m_hasAttackedInCurrentWindow)
        {
            PerformAttackDetection();
            m_hasAttackedInCurrentWindow = true;
        }

        if (m_stateTimer >= m_effectiveStats.attackDuration)
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
            if (enemyCollider.gameObject == gameObject || !enemyCollider.isTrigger)
                continue;
            
            if (enemyCollider.TryGetComponent(out IDamageable damageableEnemy))
            {
                damageableEnemy.TakeDamage(m_effectiveStats.attackDamage, transform.position);
            }
        }
    }

    private void HandleDodgeStateLogic()
    {
        if (m_stateTimer >= m_effectiveStats.dodgeDuration)
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

        if (m_stateTimer >= m_effectiveStats.takeDamageStunDuration)
        {
            // EVENTO
            OnPlayerGetHit?.Invoke();
            m_canPerformAction = true;
            if (m_currentHealth <= 0)
            {
                if (m_movementInput != Vector3.zero)
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
    }

    protected override void Die()
    {
        SetState(CharacterState.Die);

        // EVENTO
        OnPlayerGetKilled?.Invoke();

        m_rigidbody.linearVelocity = Vector3.zero;

        Debug.Log("Player has died!");

        // Inicia a contagem para reiniciar antes de desativar o script
        StartCoroutine(RestartSceneRoutine());
    }

    private System.Collections.IEnumerator RestartSceneRoutine()
    {
        // 1. Desabilita inputs e física, mas mantém o script rodando a corotina
        m_rigidbody.isKinematic = true;

        // Opcional: Desabilita colisores para inimigos pararem de bater
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 2. Espera a duração da Animação de Morte
        yield return new WaitForSeconds(_deathAnimationDuration);

        // 3. Espera os 2 segundos extras que você pediu
        yield return new WaitForSeconds(_restartDelay);

        // 4. Recarrega a cena atual
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    private void PerformMovement(Vector3 direction)
    {
        if (m_currentState == CharacterState.Attack ||
            m_currentState == CharacterState.Die)
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        if (m_currentState == CharacterState.TakeDamage)
        {
            return;
        }

        if (m_currentState == CharacterState.Dodge)
        {
            return;
        }

        // Transforma a direção local (baseada no transform do player) em direção de mundo
        Vector3 worldDirection = transform.TransformDirection(new Vector3(direction.x, 0, direction.z));

        Vector3 currentVelocity = m_rigidbody.linearVelocity;

        currentVelocity.x = worldDirection.x * m_effectiveStats.movementSpeedX;
        currentVelocity.z = worldDirection.z * m_effectiveStats.movementSpeedZ;
        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void PerformDodgeMovement()
    {
        float curveFactor = _dodgeCurve.Evaluate(m_stateTimer / m_effectiveStats.dodgeDuration);

        // CORREÇÃO AQUI:
        // O m_dodgeDirection guarda (0,0,1) se apertou W.
        // O TransformDirection converte esse (0,0,1) para a frente ATUAL do personagem no mundo.
        Vector3 worldDodgeDir = transform.TransformDirection(m_dodgeDirection);

        Vector3 currentVelocity = m_rigidbody.linearVelocity;

        // Usa a direção de MUNDO calculada acima
        currentVelocity.x = worldDodgeDir.x * m_effectiveStats.movementSpeedX * _dodgeSpeedMultiplier * curveFactor;
        currentVelocity.z = worldDodgeDir.z * m_effectiveStats.movementSpeedZ * _dodgeSpeedMultiplier * curveFactor;

        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void Attack()
    {
        SetState(CharacterState.Attack);

        // 2. Dispara evento
        OnPlayerAttack?.Invoke();

        m_canPerformAction = false;
        m_hasAttackedInCurrentWindow = false;
        
    }

    private void Dodge()
    {
        if (m_currentState == CharacterState.Attack || m_currentState == CharacterState.TakeDamage || m_currentState == CharacterState.Die) return;

        SetState(CharacterState.Dodge);

        // 2. DISPARE O EVENTO
        OnPlayerDodge?.Invoke();

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

    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (m_currentState == CharacterState.Dodge) return;

        base.TakeDamage(damage, hitSourcePosition);

        UpdateHealthUI();

        if (m_currentHealth > 0 && _impulseSource != null)
        {
            _impulseSource.GenerateImpulse();
        }
    }

    private void UpdateInteractableHighlight()
    {
        m_interactionCheckTimer -= Time.deltaTime;
        if (m_interactionCheckTimer <= 0)
        {
            m_interactionCheckTimer = _interactionCheckInterval;
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, _interactionRange, _interactableLayer);

            IInteractable closestInteractable = null;
            float minDistance = float.MaxValue;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.TryGetComponent(out IInteractable interactable))
                {
                    float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            if (closestInteractable != m_currentHighlightedInteractable)
            {
                if (m_currentHighlightedInteractable != null)
                {
                    m_currentHighlightedInteractable.HideUI();
                }

                m_currentHighlightedInteractable = closestInteractable;

                if (m_currentHighlightedInteractable != null)
                {
                    m_currentHighlightedInteractable.ShowUI();
                }
            }
        }
    }

    protected override void UpdateSpriteOrientation(Vector3 direction)
    {
        base.UpdateSpriteOrientation(direction);

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

    private void OnMove(Vector2 value)
    {
        if (m_currentState == CharacterState.Die || m_currentState == CharacterState.TakeDamage)
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
        if (m_currentState == CharacterState.Die ||
            m_currentState == CharacterState.TakeDamage)
            return;
        
        if (m_currentState == CharacterState.Attack)
            return;

        if (m_currentState == CharacterState.Dodge || m_canPerformAction)
        {
            Attack();
        }
    }

    private void OnDodge()
    {
        if (m_currentState == CharacterState.Die ||
            m_currentState == CharacterState.Attack ||
            m_currentState == CharacterState.TakeDamage)
            return;

        if (m_canPerformAction)
            Dodge();
    }

    private void OnInteract()
    {
        if (m_currentState == CharacterState.Die ||
            m_currentState == CharacterState.Attack ||
            m_currentState == CharacterState.TakeDamage)
            return;

        if (m_canPerformAction && m_currentHighlightedInteractable != null)
        {
            m_currentHighlightedInteractable.Interact(gameObject);
            UpdateInteractableHighlight(); 
        }
    }

    void OnEnable()
    {
        Manager_Events.Input.OnMove += OnMove;
        Manager_Events.Input.OnAttack += OnAttack;
        Manager_Events.Input.OnDodge += OnDodge;
        Manager_Events.Input.OnInteract += OnInteract;

        // Inscreve no novo evento de rotação
        WorldRotationManager.OnRotateRequest += RotatePlayer;
    }

    void OnDisable()
    {
        Manager_Events.Input.OnMove -= OnMove;
        Manager_Events.Input.OnAttack -= OnAttack;
        Manager_Events.Input.OnDodge -= OnDodge;
        Manager_Events.Input.OnInteract -= OnInteract;

        // DesInscreve no novo evento de rotação
        WorldRotationManager.OnRotateRequest -= RotatePlayer;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }


    private void RotatePlayer(float angle)
    {
        // Não gira se estiver morto, atacando ou JÁ girando
        if (m_currentState == CharacterState.Die || _isRotating) return;

        StartCoroutine(RotateSmoothlyRoutine(angle));
    }

    private System.Collections.IEnumerator RotateSmoothlyRoutine(float angle)
    {
        _isRotating = true;

        Quaternion startRotation = transform.rotation;
        // Calcula a rotação alvo somando ao que já existe
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, angle, 0);

        float elapsed = 0f;

        while (elapsed < _rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _rotationDuration;

            // Slerp faz a interpolação esférica (suave)
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        // Garante que termine exatamente no ângulo certo (evita erros flutuantes)
        transform.rotation = targetRotation;
        _isRotating = false;
    }

    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            // Garante que a barra saiba qual é a vida máxima atual (caso mude por buffs/equipamentos)
            _healthSlider.maxValue = m_effectiveStats.maxHealth;
            _healthSlider.value = m_currentHealth;
        }
    }

}