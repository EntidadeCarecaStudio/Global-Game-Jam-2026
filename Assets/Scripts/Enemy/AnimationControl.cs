using UnityEngine;

public class AnimationControl : MonoBehaviour
{

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private RuntimeAnimatorController controller;
    
    private Animator anim;
    private Enemy enemy;

    private void Start()
    {

        anim = GetComponent<Animator>();
        enemy = GetComponentInParent<Enemy>();
        anim.runtimeAnimatorController = controller;
        
    }

    public void PlayAnim(int value)
    {
        anim.SetInteger("trasition", value);
    }

    public void Attack()
    {
        if (!enemy.isDead)
        {
            Collider[] hit = Physics.OverlapSphere(attackPoint.position, radius, playerLayer);
            foreach (Collider playerCollider in hit)
            {
                if (playerCollider.gameObject == gameObject || !playerCollider.isTrigger)
                    continue;

                if (playerCollider.TryGetComponent(out IDamageable damageableEnemy))
                {
                    damageableEnemy.TakeDamage(10, transform.position);
                }
            }
        }

    }

    public void OnHit()
    {
        

        if(enemy.health <= 0)
        {
            enemy.isDead = true;
            anim.SetTrigger("death");

            Destroy(enemy.gameObject, 2f);
        }
        else
        {
            anim.SetTrigger("hit");
            enemy.health--;
        }

    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, radius);
    }



}
