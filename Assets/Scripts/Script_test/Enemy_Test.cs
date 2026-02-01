using UnityEngine;

using UnityEngine;

public class AttackEnemy : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.0f;

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private bool canAttack = true;
    private float attackTimer = 0f;

    void Start()
    {
        if (attackPoint == null)
        {
            // Se não houver attackPoint definido, usa o transform do próprio objeto
            attackPoint = transform;
            Debug.LogWarning("Attack point not set. Using enemy's transform as attack point.");
        }
    }

    void Update()
    {
        // Temporizador para cooldown do ataque
        if (!canAttack)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                canAttack = true;
                attackTimer = 0f;
            }
        }

        // Se pode atacar, tenta realizar o ataque
        if (canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // Detecta o jogador na área de ataque
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);

        if (hitPlayer != null)
        {
            // Tenta causar dano ao jogador
            if (hitPlayer.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage, transform.position);
                Debug.Log($"Enemy attacked player for {attackDamage} damage.");
            }
            else
            {
                Debug.LogWarning("Player does not have an IDamageable component.");
            }

            // Inicia o cooldown
            canAttack = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmos && attackPoint != null)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
