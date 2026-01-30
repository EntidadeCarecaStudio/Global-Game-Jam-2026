using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_DashAttackData", menuName = "Scriptable Objects/Attacks/Dash Attack Data")]
public class SO_DashAttackData : SO_AttackData
{
    [Header("Dash Parameters")]
    public AnimationClip dashAttackAnimationClip;
    public float dashDuration = 0.6f;
    public float speedMultiplier = 2.5f;

    public override void Execute(AttackContext context)
    {
        context.attacker.GetComponent<MinibossController>().Animator.PlayDashAttack(dashDuration, dashAttackAnimationClip);
        context.runner.StartCoroutine(DashRoutine(context));
    }

    private IEnumerator DashRoutine(AttackContext context)
    {
        float baseSpeed = context.agent.speed;
        context.agent.speed *= speedMultiplier;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            context.agent.SetDestination(context.target.position);
            elapsed += Time.deltaTime;
            yield return null;
        }

        context.agent.speed = baseSpeed;
    }
}
