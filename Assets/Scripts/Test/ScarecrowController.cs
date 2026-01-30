using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScarecrowController : BaseCharacterController
{

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private bool _enableKnockback = true;
    [SerializeField] private float _regenerationDelay = 3.0f;
    [SerializeField] private float _regenerationRate = 20.0f;
    [SerializeField] private float _knockbackRevertTime = 0.1f;
    
    private float m_regenerationTimer;
    private Vector3 m_initialPosition;
    private Quaternion m_initialRotation;
    private Coroutine m_knockbackCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        m_rigidbody.isKinematic = true;
        m_initialPosition = transform.position;
        m_initialRotation = transform.rotation;
    }

    protected override void Start()
    {
        base.Start();

        UpdateHealthUI();
        m_regenerationTimer = _regenerationDelay;
    }

    protected override void Update()
    {
        base.Update();
        HandleRegeneration();

        if (m_rigidbody.isKinematic)
        {
            transform.position = m_initialPosition;
            transform.rotation = m_initialRotation;
        }
    }

    private void HandleRegeneration()
    {
        if (m_currentHealth >= m_effectiveStats.maxHealth) return;

        if (m_regenerationTimer > 0)
        {
            m_regenerationTimer -= Time.deltaTime;
        }
        else
        {
            m_currentHealth += Mathf.CeilToInt(_regenerationRate * Time.deltaTime);
            if (m_currentHealth > m_effectiveStats.maxHealth)
            {
                m_currentHealth = m_effectiveStats.maxHealth;
            }
            UpdateHealthUI();
        }
    }

    public override void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        m_currentHealth -= damage;
        if (m_currentHealth < 1)
        {
            m_currentHealth = 1; 
        }
        UpdateHealthUI();

        m_regenerationTimer = _regenerationDelay;

        if (_enableKnockback)
        {
            if (m_knockbackCoroutine != null) StopCoroutine(m_knockbackCoroutine);
            m_knockbackCoroutine = StartCoroutine(ApplyKnockbackAndReset(hitSourcePosition));
        }

        if (m_damageFeedbackCoroutine != null) StopCoroutine(m_damageFeedbackCoroutine);
        m_damageFeedbackCoroutine = StartCoroutine(DamageFeedback());
    }

    private IEnumerator ApplyKnockbackAndReset(Vector3 hitSourcePosition)
    {
        m_rigidbody.isKinematic = false;
        m_rigidbody.linearVelocity = Vector3.zero;

        Vector3 knockbackDirection = (transform.position - hitSourcePosition).normalized;
        Vector3 flatKnockbackDirection = new Vector3(knockbackDirection.x, 0f, knockbackDirection.z).normalized;
        
        float finalKnockbackForce = m_effectiveStats.knockbackForce * (1f - m_effectiveStats.knockbackResistance);
        m_rigidbody.AddForce(flatKnockbackDirection * finalKnockbackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(_knockbackRevertTime);

        m_rigidbody.isKinematic = true;
        transform.position = m_initialPosition;
        transform.rotation = m_initialRotation;
    }

    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = m_effectiveStats.maxHealth;
            _healthSlider.value = m_currentHealth;
        }
    }

    private new IEnumerator DamageFeedback()
    {
        if (_spriteRenderer != null)
        {
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(m_effectiveStats.damageFeedbackDuration);
            _spriteRenderer.color = originalColor;
        }
    }

    protected override void Die()
    {
        SetState(CharacterState.Die);
        Debug.Log("Scarecrow cannot die! Its current health is fixed at 1.");
    }
    
}