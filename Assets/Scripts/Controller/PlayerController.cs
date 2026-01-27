using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _movementSpeedX = 5.0f;
    [SerializeField] private float _movementSpeedZ = 3.0f;
    [SerializeField] private float _dodgeSpeedMultiplier = 2.0f;
    [SerializeField] private float _dodgeDuration = 0.5f;
    [SerializeField] private AnimationCurve _dodgeCurve;
    [SerializeField] private float _attackDuration = 0.4f;
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private CharacterAnimationController _characterAnimationController;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private int m_currentHealth;
    private CharacterState m_currentState;
    private Vector3 m_movementInput;
    private Vector3 m_dodgeDirection;
    private float m_stateTimer;
    private bool m_canPerformAction = true;
    private Rigidbody m_rigidbody;
    private Vector3 m_facingDirection = Vector3.back;

    public int CurrentHealth => m_currentHealth;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;
    }

    private void Start()
    {
        m_currentHealth = _maxHealth;
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
            if (_characterAnimationController != null)
            {
                _characterAnimationController.UpdateAnimation(m_currentState);
            }
        }
    }

    private void HandleInput()
    {
        if (m_currentState == CharacterState.Die)
        {
            m_movementInput = Vector3.zero;
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        m_movementInput = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (m_movementInput.x != 0)
        {
            m_facingDirection = new Vector3(m_movementInput.x, 0f, 0f).normalized;
        }

        if (m_canPerformAction)
        {
            if (Input.GetButton("Attack"))
            {
                Attack();
            }
            else if (Input.GetButton("Dodge"))
            {
                Dodge();
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
        if (m_stateTimer >= _attackDuration)
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

    private void HandleDodgeStateLogic()
    {
        if (m_stateTimer >= _dodgeDuration)
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

        currentVelocity.x = direction.x * _movementSpeedX;
        currentVelocity.z = direction.z * _movementSpeedZ;

        m_rigidbody.linearVelocity = currentVelocity;
    }

    private void PerformDodgeMovement()
    {
        float curveFactor = _dodgeCurve.Evaluate(m_stateTimer / _dodgeDuration);

        Vector3 currentVelocity = m_rigidbody.linearVelocity;

        currentVelocity.x = m_dodgeDirection.x * _movementSpeedX * _dodgeSpeedMultiplier * curveFactor;
        currentVelocity.z = m_dodgeDirection.z * _movementSpeedZ * _dodgeSpeedMultiplier * curveFactor;

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

}