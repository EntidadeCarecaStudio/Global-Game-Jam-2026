using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct EffectiveStats
{
    public int maxHealth;
    public int attackDamage;
    public float movementSpeedX;
    public float movementSpeedZ;
    public float attackDuration;
    public float dodgeDuration;
    public float attackCooldown;
    public float takeDamageStunDuration;
    public float damageFeedbackDuration;
    public float knockbackForce;
    public float knockbackResistance;
}

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseCharacterController : MonoBehaviour, IDamageable
{
    [SerializeField] protected CharacterStats _baseCharacterStats;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected CharacterAnimationController _characterAnimationController;

    protected Rigidbody m_rigidbody;
    protected int m_currentHealth;
    protected CharacterState m_currentState;
    protected float m_stateTimer;
    protected Coroutine m_damageFeedbackCoroutine;
    protected EffectiveStats m_effectiveStats;

    protected MaskData m_currentEquippedMask;

    public int CurrentHealth => m_currentHealth;
    public CharacterState CurrentState => m_currentState;

    protected virtual void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.freezeRotation = true;
    }

    protected virtual void Start()
    {
        if (_baseCharacterStats == null)
        {
            Debug.LogError("BaseCharacterStats ScriptableObject not assigned to " + gameObject.name);
            enabled = false;
            return;
        }

        CalculateEffectiveStats();
        m_currentHealth = m_effectiveStats.maxHealth;
        SetState(CharacterState.Idle);
    }

    protected virtual void Update()
    {
        m_stateTimer += Time.deltaTime;
    }

    protected void CalculateEffectiveStats()
    {
        m_effectiveStats = new EffectiveStats
        {
            maxHealth = _baseCharacterStats.maxHealth,
            attackDamage = _baseCharacterStats.attackDamage,
            movementSpeedX = _baseCharacterStats.movementSpeedX,
            movementSpeedZ = _baseCharacterStats.movementSpeedZ,
            attackDuration = _baseCharacterStats.attackDuration,
            dodgeDuration = _baseCharacterStats.dodgeDuration,
            attackCooldown = _baseCharacterStats.attackCooldown,
            takeDamageStunDuration = _baseCharacterStats.takeDamageStunDuration,
            damageFeedbackDuration = _baseCharacterStats.damageFeedbackDuration,
            knockbackForce = _baseCharacterStats.knockbackForce,
            knockbackResistance = _baseCharacterStats.knockbackResistance
        };

        if (m_currentEquippedMask != null)
        {
            m_effectiveStats.maxHealth += m_currentEquippedMask.maxHealthModifier;
            m_effectiveStats.attackDamage += m_currentEquippedMask.attackDamageModifier;
            m_effectiveStats.movementSpeedX += m_currentEquippedMask.movementSpeedXModifier;
            m_effectiveStats.movementSpeedZ += m_currentEquippedMask.movementSpeedZModifier;
            m_effectiveStats.attackDuration += m_currentEquippedMask.attackDurationModifier;
            m_effectiveStats.dodgeDuration += m_currentEquippedMask.dodgeDurationModifier;
            m_effectiveStats.attackCooldown += m_currentEquippedMask.attackCooldownModifier;
            m_effectiveStats.takeDamageStunDuration += m_currentEquippedMask.takeDamageStunDurationModifier;
            m_effectiveStats.knockbackForce += m_currentEquippedMask.knockbackForceModifier;
            m_effectiveStats.knockbackResistance = Mathf.Clamp01(m_effectiveStats.knockbackResistance + m_currentEquippedMask.knockbackResistanceModifier);
        }

        m_effectiveStats.maxHealth = Mathf.Max(1, m_effectiveStats.maxHealth);
        m_effectiveStats.attackDuration = Mathf.Max(0.05f, m_effectiveStats.attackDuration);
        m_effectiveStats.dodgeDuration = Mathf.Max(0.05f, m_effectiveStats.dodgeDuration);
        m_effectiveStats.attackCooldown = Mathf.Max(0f, m_effectiveStats.attackCooldown);
        m_effectiveStats.takeDamageStunDuration = Mathf.Max(0f, m_effectiveStats.takeDamageStunDuration);
        m_effectiveStats.knockbackForce = Mathf.Max(0f, m_effectiveStats.knockbackForce);
    }

    public void EquipMask(MaskData newMask)
    {
        if (newMask == null) return;

        m_currentEquippedMask = newMask;
        CalculateEffectiveStats();
        if (m_currentHealth > m_effectiveStats.maxHealth)
        {
            m_currentHealth = m_effectiveStats.maxHealth;
        }
    }

    public void UnequipCurrentMask()
    {
        if (m_currentEquippedMask == null) return;

        m_currentEquippedMask = null;
        CalculateEffectiveStats();
        if (m_currentHealth > m_effectiveStats.maxHealth)
        {
            m_currentHealth = m_effectiveStats.maxHealth;
        }
    }

    protected void SetState(CharacterState newState)
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

    public virtual void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (m_currentState == CharacterState.Die) return;
        
        float effectiveDamage = damage;
        
        m_currentHealth -= Mathf.RoundToInt(effectiveDamage);

        if (m_currentHealth <= 0)
        {
            m_currentHealth = 0;
            Die();
        }
        else
        {
            SetState(CharacterState.TakeDamage);
            
            Vector3 knockbackDirection = (transform.position - hitSourcePosition).normalized;
            Vector3 flatKnockbackDirection = new Vector3(knockbackDirection.x, 0f, knockbackDirection.z).normalized;
            
            float finalKnockbackForce = m_effectiveStats.knockbackForce * (1f - m_effectiveStats.knockbackResistance);

            if (finalKnockbackForce > 0.01f && m_rigidbody != null)
            {
                m_rigidbody.AddForce(flatKnockbackDirection * finalKnockbackForce, ForceMode.Impulse);
            }

            if (m_damageFeedbackCoroutine != null) StopCoroutine(m_damageFeedbackCoroutine);
            m_damageFeedbackCoroutine = StartCoroutine(DamageFeedback());
        }
    }

    protected IEnumerator DamageFeedback()
    {
        if (_spriteRenderer != null)
        {
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(m_effectiveStats.damageFeedbackDuration);
            _spriteRenderer.color = originalColor;
        }
    }

    protected virtual void UpdateSpriteOrientation(Vector3 direction)
    {
        if (_spriteRenderer == null) return;

        if (direction.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
    }

    protected abstract void Die();
}