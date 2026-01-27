using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    
    private enum PlayerState
    {
        Idle,
        Run,
        Attack,
        Dodge,
        TakeDamage,
        Die
    }

    [SerializeField] private float _movementSpeed = 5.0f;
    [SerializeField] private float _dodgeSpeedMultiplier = 2.0f;
    [SerializeField] private float _dodgeDuration = 0.5f;
    [SerializeField] private float _attackDuration = 0.4f;
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private Animator _animator;

    private int m_currentHealth;
    private PlayerState m_currentState;
    private Vector3 m_movementInput;
    private float m_stateTimer;
    private bool m_canPerformAction = true;
    private Rigidbody m_rigidbody;
    private Vector3 m_facingDirection = Vector3.right;

    public int CurrentHealth => m_currentHealth;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        m_currentHealth = _maxHealth;
        SetState(PlayerState.Idle);
    }

    private void Update()
    {
        HandleInput();
        UpdateStateLogic();
    }

    private void FixedUpdate()
    {
        PerformMovement(m_movementInput);
    }

    private void SetState(PlayerState newState)
    {
        m_currentState = newState;
        m_stateTimer = 0f;
        UpdateAnimator();
    }

    private void HandleInput()
    {
        if (m_currentState == PlayerState.Die)
        {
            m_movementInput = Vector3.zero;
            return;
        }

        float horizontalInput = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? -1f :
                                Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 
                                0f;
        float verticalInput = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? -1f :
                              Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 
                              0f;

        m_movementInput = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (m_canPerformAction)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                Attack();
            }
            else if (Input.GetKey(KeyCode.Space))
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
            case PlayerState.Idle:
            case PlayerState.Run:
                if (m_movementInput != Vector3.zero)
                {
                    SetState(PlayerState.Run);
                    m_facingDirection = m_movementInput;
                }
                else
                {
                    SetState(PlayerState.Idle);
                }
                break;

            case PlayerState.Attack:
                if (m_stateTimer >= _attackDuration)
                {
                    m_canPerformAction = true;
                    if (m_movementInput != Vector3.zero)
                    {
                        SetState(PlayerState.Run);
                    }
                    else
                    {
                        SetState(PlayerState.Idle);
                    }
                }
                break;

            case PlayerState.Dodge:
                if (m_stateTimer >= _dodgeDuration)
                {
                    m_canPerformAction = true;
                    if (m_movementInput != Vector3.zero)
                    {
                        SetState(PlayerState.Run);
                    }
                    else
                    {
                        SetState(PlayerState.Idle);
                    }
                }
                PerformDodgeMovement();
                break;

            case PlayerState.TakeDamage:
                if (m_stateTimer >= 0.3f)
                {
                    if (m_currentHealth <= 0)
                    {
                        Die();
                    }
                    else
                    {
                        SetState(PlayerState.Idle);
                    }
                }
                break;

            case PlayerState.Die:
                break;
        }
    }

    private void PerformMovement(Vector3 direction)
    {
        if (m_currentState == PlayerState.Dodge) return;
        if (m_currentState == PlayerState.Attack ||
            m_currentState == PlayerState.TakeDamage ||
            m_currentState == PlayerState.Die)
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            return;
        }

        m_rigidbody.linearVelocity = direction * _movementSpeed;
    }

    private void PerformDodgeMovement()
    {
        m_rigidbody.linearVelocity = m_facingDirection * _movementSpeed * _dodgeSpeedMultiplier;
    }

    private void Attack()
    {
        if (m_currentState == PlayerState.Attack || m_currentState == PlayerState.Dodge ||
            m_currentState == PlayerState.TakeDamage || m_currentState == PlayerState.Die) return;

        SetState(PlayerState.Attack);
        m_canPerformAction = false;
    }

    private void Dodge()
    {
        if (m_currentState == PlayerState.Attack || m_currentState == PlayerState.Dodge ||
            m_currentState == PlayerState.TakeDamage || m_currentState == PlayerState.Die) return;

        SetState(PlayerState.Dodge);
        m_canPerformAction = false;
    }

    public void TakeDamage(int damage)
    {
        if (m_currentState == PlayerState.Die) return;
        if (m_currentState == PlayerState.Dodge) return;

        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            m_currentHealth = 0;
            Die();
        }
        else
        {
            SetState(PlayerState.TakeDamage);
            StartCoroutine(DamageFeedback());
        }
    }

    private void Die()
    {
        SetState(PlayerState.Die);
        Debug.Log("Player has died!");
        enabled = false;
    }

    private void UpdateAnimator()
    {
        if (_animator == null) return;

        _animator.ResetTrigger("Idle");
        _animator.ResetTrigger("Run");
        _animator.ResetTrigger("Attack");
        _animator.ResetTrigger("Dodge");
        _animator.ResetTrigger("TakeDamage");
        _animator.ResetTrigger("Die");

        switch (m_currentState)
        {
            case PlayerState.Idle:
                _animator.SetBool("IsRunning", false);
                break;
            case PlayerState.Run:
                _animator.SetBool("IsRunning", true);
                _animator.SetFloat("MoveX", m_movementInput.x);
                _animator.SetFloat("MoveY", m_movementInput.y);
                break;
            case PlayerState.Attack:
                _animator.SetBool("IsRunning", false);
                _animator.SetTrigger("Attack");
                break;
            case PlayerState.Dodge:
                _animator.SetBool("IsRunning", false);
                _animator.SetTrigger("Dodge");
                break;
            case PlayerState.TakeDamage:
                _animator.SetBool("IsRunning", false);
                _animator.SetTrigger("TakeDamage");
                break;
            case PlayerState.Die:
                _animator.SetBool("IsRunning", false);
                _animator.SetTrigger("Die");
                break;
        }

        if (m_movementInput != Vector3.zero)
        {
             _animator.SetFloat("LastMoveX", m_movementInput.x);
             _animator.SetFloat("LastMoveY", m_movementInput.y);
        }
        else if (m_currentState == PlayerState.Idle)
        {
            _animator.SetFloat("LastMoveX", m_facingDirection.x);
            _animator.SetFloat("LastMoveY", m_facingDirection.y);
        }
    }

    private IEnumerator DamageFeedback()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
}