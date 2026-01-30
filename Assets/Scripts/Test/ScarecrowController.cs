using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Necessário para Slider

[RequireComponent(typeof(Rigidbody), typeof(Collider))] // Garante Rigidbody e Collider
public class ScarecrowController : MonoBehaviour, IDamageable
{
    [SerializeField] private CharacterStats _scarecrowStats; // Usaremos CharacterStats para MaxHealth e KnockbackForce
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Slider _healthSlider; // UI para mostrar HP, agora um Slider
    [SerializeField] private bool _enableKnockback = true; // Opção configurável para knockback
    [SerializeField] private float _regenerationDelay = 3.0f; // Tempo sem receber dano para começar a regenerar
    [SerializeField] private float _regenerationRate = 20.0f; // HP por segundo regenerado
    [SerializeField] private float _knockbackRevertTime = 0.1f; // Tempo para o Rigidbody voltar a ser kinematic após knockback
    
    private int m_currentHealth;
    private float m_regenerationTimer;
    private Rigidbody m_rigidbody;
    private Vector3 m_initialPosition; // Para manter o espantalho "parado"
    private Quaternion m_initialRotation;
    private Coroutine m_damageFeedbackCoroutine;
    private Coroutine m_knockbackCoroutine; // Para controlar o tempo de knockback

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_rigidbody.useGravity = false; // Espantalho não cai
        m_rigidbody.constraints = RigidbodyConstraints.FreezeRotation; // Não rotaciona
        
        // O Rigidbody do espantalho sempre começa como cinemático para ser imóvel
        m_rigidbody.isKinematic = true; 

        m_initialPosition = transform.position;
        m_initialRotation = transform.rotation;
    }

    private void Start()
    {
        if (_scarecrowStats == null)
        {
            Debug.LogError("ScarecrowStats (CharacterStats ScriptableObject) not assigned to ScarecrowController.");
            enabled = false;
            return;
        }

        m_currentHealth = _scarecrowStats.maxHealth;
        UpdateHealthUI();
        m_regenerationTimer = _regenerationDelay; // Começa com o timer cheio
    }

    private void Update()
    {
        HandleRegeneration();
        // Não é mais necessário orientar o Slider para a câmera,
        // pois ele fará parte de um Canvas que gerencia a orientação.

        // Se o espantalho não está em knockback ativo, garante que sua posição não seja alterada
        // por colisões e que ele retorne à sua posição inicial.
        // Esta parte agora é crucial mesmo com isKinematic = true, para garantir 100% que ele não se mova
        // se algo inesperado acontecer ou se o Rigidbody temporariamente foi non-kinematic.
        if (m_rigidbody.isKinematic)
        {
            // transform.position = m_initialPosition;
            // transform.rotation = m_initialRotation;
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void HandleRegeneration()
    {
        if (m_currentHealth >= _scarecrowStats.maxHealth) return; // Se já estiver full HP, não precisa regenerar

        if (m_regenerationTimer > 0)
        {
            m_regenerationTimer -= Time.deltaTime;
        }
        else // Começa a regenerar quando o timer zera
        {
            m_currentHealth += Mathf.CeilToInt(_regenerationRate * Time.deltaTime);
            if (m_currentHealth > _scarecrowStats.maxHealth)
            {
                m_currentHealth = _scarecrowStats.maxHealth;
            }
            UpdateHealthUI();
        }
    }

    public void TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (_scarecrowStats == null) return; // Garantia para evitar NullReferenceException

        // Espantalho é imortal, HP não pode ser menor que 1
        m_currentHealth -= damage;
        if (m_currentHealth <= 0)
        {
            m_currentHealth = 1; // Garante que nunca morra
        }
        UpdateHealthUI();

        m_regenerationTimer = _regenerationDelay; // Reseta o timer de regeneração

        if (_enableKnockback)
        {
            // Para garantir que o espantalho receba o knockback, ele precisa ser temporariamente não cinemático.
            if (m_knockbackCoroutine != null) StopCoroutine(m_knockbackCoroutine);
            m_knockbackCoroutine = StartCoroutine(ApplyKnockbackAndReset(hitSourcePosition));
        }
        // Se _enableKnockback for false, o Rigidbody já é cinemático e não se moverá.
        // O bloco else anterior foi removido pois a nova corotina ou o Update já garantem a imobilidade.

        // Feedback visual (sprite vermelho)
        if (m_damageFeedbackCoroutine != null) StopCoroutine(m_damageFeedbackCoroutine);
        m_damageFeedbackCoroutine = StartCoroutine(DamageFeedback());
    }

    private IEnumerator ApplyKnockbackAndReset(Vector3 hitSourcePosition)
    {
        // Torna o Rigidbody não cinemático para receber a força
        m_rigidbody.isKinematic = false;
        m_rigidbody.linearVelocity = Vector3.zero; // Zera qualquer velocidade anterior para um knockback limpo

        // Aplica o knockback
        Vector3 knockbackDirection = (transform.position - hitSourcePosition).normalized;
        Vector3 flatKnockbackDirection = new Vector3(knockbackDirection.x, 0f, knockbackDirection.z).normalized;
        m_rigidbody.AddForce(flatKnockbackDirection * _scarecrowStats.knockbackForce, ForceMode.Impulse);

        yield return new WaitForSeconds(_knockbackRevertTime); // Espera um tempo curto para a força ser aplicada

        // Retorna o Rigidbody para cinemático e reseta a posição
        m_rigidbody.isKinematic = true;
        m_rigidbody.linearVelocity = Vector3.zero;
        m_rigidbody.angularVelocity = Vector3.zero;
        // transform.position = m_initialPosition; // Volta para a posição inicial exata
        // transform.rotation = m_initialRotation;
    }

    private void UpdateHealthUI()
    {
        if (_healthSlider != null)
        {
            _healthSlider.maxValue = _scarecrowStats.maxHealth;
            _healthSlider.value = m_currentHealth;
        }
    }

    private IEnumerator DamageFeedback()
    {
        if (_spriteRenderer != null && _scarecrowStats != null)
        {
            Color originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(_scarecrowStats.damageFeedbackDuration);
            _spriteRenderer.color = originalColor;
        }
    }
}