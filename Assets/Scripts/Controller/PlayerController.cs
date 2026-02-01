using UnityEngine;
using Unity.Cinemachine;
using System.Runtime.InteropServices;

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

    [Header("Cinemachine Impulse")]
    [SerializeField] private CinemachineImpulseSource _impulseSource;

    private Vector3 m_movementInput;
    private Vector3 m_dodgeDirection;
    private bool m_canPerformAction = true;
    private Vector3 m_facingDirection = Vector3.back;
    private bool m_hasAttackedInCurrentWindow;

    private IInteractable m_currentHighlightedInteractable;
    private float m_interactionCheckTimer;

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
        SetState(CharacterState.Idle);
    }

    protected override void Update()
    {
        base.Update();
        UpdateStateLogic();
        UpdateSpriteOrientation(m_facingDirection);
        UpdateInteractableHighlight();

        // Mecanica de Giro
        HandleRotationInput();
    }

    // Mecanica de Giro
    private void HandleRotationInput()
    {
        // Se apertar Q, gira -90 (Esquerda)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            BaseCharacterController.TriggerRotation(-90f);
        }
        // Se apertar E, gira +90 (Direita)
        else if (Input.GetKeyDown(KeyCode.E))
        {
            BaseCharacterController.TriggerRotation(90f);
        }
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
        m_rigidbody.linearVelocity = Vector3.zero;
        enabled = false;
        Debug.Log("Player has died!");
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

        // --- A MUDANÇA COMEÇA AQUI ---

        // 2. Calculamos a velocidade desejada RELATIVA ao input
        // (Ex: Se apertou W, quer se mover na velocidade Z para frente)
        Vector3 localVelocity = new Vector3(
            direction.x * m_effectiveStats.movementSpeedX, // Velocidade lateral (Strafe)
            0f,
            direction.z * m_effectiveStats.movementSpeedZ  // Velocidade frontal
        );

        // 3. Convertemos de Local (Input) para Global (Mundo)
        // O TransformDirection pega o vetor e aplica a rotação atual do Player nele.
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // 4. Preservamos a gravidade (velocidade Y atual)
        globalVelocity.y = m_rigidbody.linearVelocity.y;

        // 5. Aplicamos no Rigidbody
        m_rigidbody.linearVelocity = globalVelocity;
    }

    private void PerformDodgeMovement()
    {
        // 1. Obtém o fator da curva de animação
        float curveFactor = _dodgeCurve.Evaluate(m_stateTimer / m_effectiveStats.dodgeDuration);
        
        // 2. Calcula a velocidade LOCAL (Relativa ao corpo do personagem)
        // Aqui assumimos que m_dodgeDirection já é um vetor normalizado de input (ex: 0,0,1 para frente)
        Vector3 localDodgeVelocity = new Vector3(
            m_dodgeDirection.x * m_effectiveStats.movementSpeedX * _dodgeSpeedMultiplier * curveFactor, 0f, // Y é zero no local space para não voar para cima
            m_dodgeDirection.z * m_effectiveStats.movementSpeedZ * _dodgeSpeedMultiplier * curveFactor
        );

        // 3. Converte de Local para Global baseado na rotação atual do Player
        // Isso garante que se o player girou 90 graus, a esquiva "para frente" acompanha o giro.
        Vector3 globalDodgeVelocity = transform.TransformDirection(localDodgeVelocity);

        // 4. Preserva a gravidade atual (Velocidade Y do mundo)
        globalDodgeVelocity.y = m_rigidbody.linearVelocity.y;

        // 5. Aplica a velocidade final
        m_rigidbody.linearVelocity = globalDodgeVelocity;
    }

    private void Attack()
    {
        SetState(CharacterState.Attack);
        m_canPerformAction = false;
        m_hasAttackedInCurrentWindow = false;
    }

    private void Dodge()
    {
        if (m_currentState == CharacterState.Attack || m_currentState == CharacterState.TakeDamage || m_currentState == CharacterState.Die) return;

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

    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (m_currentState == CharacterState.Dodge) return;

        base.TakeDamage(damage, hitSourcePosition);

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

    protected override void OnEnable()
    {
        //Pegar Eventos do BaseCharacterController
        base.OnEnable();

        Manager_Events.Input.OnMove += OnMove;
        Manager_Events.Input.OnAttack += OnAttack;
        Manager_Events.Input.OnDodge += OnDodge;
        Manager_Events.Input.OnInteract += OnInteract;
    }

    protected override void OnDisable()
    {
        //Pegar Eventos do BaseCharacterController
        base.OnDisable();

        Manager_Events.Input.OnMove -= OnMove;
        Manager_Events.Input.OnAttack -= OnAttack;
        Manager_Events.Input.OnDodge -= OnDodge;
        Manager_Events.Input.OnInteract -= OnInteract;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionRange);
    }
}