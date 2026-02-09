using UnityEngine;

public class MinibossAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private AnimatorOverrideController overrideController;

    void Awake()
    {
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    public void PlayIdle()
    {
        animator.SetBool("b_isRunning", false);
        animator.Play("Idle");
    }
    public void PlayRun() => animator.SetBool("b_isRunning", true);

    public void PlayAttack(SO_AttackData currentAttack)
    {
        overrideController["AttackClipPlaceholder"] = currentAttack.attackAnimationClip;

        if (currentAttack is SO_DashAttackData dashAttack)
        {
            animator.speed = currentAttack.attackAnimationClip.length / dashAttack.dashDuration;
        }
        else animator.speed = 1f;

        animator.Play("Attack");
    }

    public void PlayHit()
    {
        animator.SetBool("b_isRunning", false);
        animator.Play("TakeHit");
    }
    public void PlayDie() => animator.Play("Die");
}
