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

    public void PlayAttack()
    {
        animator.SetTrigger("t_onAttack");
    }
}
