using UnityEngine;

public class MinibossAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;


    public void PlayIdle()
    {
        animator.SetBool("b_isRunning", false);
    }
    public void PlayRun()
    {
        animator.SetBool("b_isRunning", true);
    }

    public void PlayMeleeAttack()
    {
        animator.SetTrigger("t_onMeleeAttack");
    }

    public void PlayDashAttack(float dashDuration, AnimationClip clip)
    {
        animator.speed = clip.length / dashDuration;
        animator.Play("DashAttack");
        //animator.SetTrigger("t_onDashAttack");
    }
}
