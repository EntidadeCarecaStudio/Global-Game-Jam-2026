using UnityEngine;
using Unity.Cinemachine;

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
    }

    private void FixedUpdate()
    {
        PerformMovement(m_movementInput);
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

        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = direction.x * m_effectiveStats.movementSpeedX;
        currentVelocity.z = direction.z * m_effectiveStats.movementSpeedZ;
        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void PerformDodgeMovement()
    {
        float curveFactor = _dodgeCurve.Evaluate(m_stateTimer / m_effectiveStats.dodgeDuration);
        
        Vector3 currentVelocity = m_rigidbody.linearVelocity;
        currentVelocity.x = m_dodgeDirection.x * m_effectiveStats.movementSpeedX * _dodgeSpeedMultiplier * curveFactor;
        currentVelocity.z = m_dodgeDirection.z * m_effectiveStats.movementSpeedZ * _dodgeSpeedMultiplier * curveFactor;
        m_rigidbody.linearVelocity = currentVelocity;
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

    void OnEnable()
    {
        Manager_Events.Input.OnMove += OnMove;
        Manager_Events.Input.OnAttack += OnAttack;
        Manager_Events.Input.OnDodge += OnDodge;
        Manager_Events.Input.OnInteract += OnInteract;
    }

    void OnDisable()
    {
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