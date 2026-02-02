using UnityEngine;

public class AnimationControl : MonoBehaviour
{

    [SerializeField] private Transform attackPoint;
    [SerializeField] private float radius;
    [SerializeField] private LayerMask playerLayer;

    private PlayerController player;
    private Animator anim;
    private Enemy enemy;

    private void Start()
    {

        anim = GetComponent<Animator>();
        enemy = GetComponentInParent<Enemy>();
        
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
            if (hit != null)
            {
               
                Debug.Log("Está batendo no player");
            }
        }

    }

    public void OnHit()
    {
        

        if(enemy.health <= 0)
        {
            enemy.isDead = true;
            anim.SetTrigger("death");
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
