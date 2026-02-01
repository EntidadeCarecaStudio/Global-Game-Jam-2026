using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_DashAttackData", menuName = "Scriptable Objects/Attacks/Dash Attack Data")]
public class SO_DashAttackData : SO_AttackData
{
    [Header("Dash Parameters")]
    public float dashDuration = 0.6f;
    public float speedMultiplier = 3f;

    public override void Execute(AttackContext context)
    {
        if (context.agent.enabled)
            context.agent.isStopped = false;

        context.executor.StartCoroutine(DashRoutine(context));
    }

    private IEnumerator DashRoutine(AttackContext context)
    {
        //float baseSpeed = context.attacker.gameObject.GetComponent<MinibossController>().StatsBinder.Stats.moveSpeed;
        float baseSpeed = context.attacker.gameObject.GetComponent<MinibossController>().StatsBinder.CStats.movementSpeedX;
        context.agent.speed *= speedMultiplier;

        float timer = 0f;
        while (timer < dashDuration)
        {
            context.agent.SetDestination(context.target.position);

            timer += Time.deltaTime;
            yield return null;
        }

        context.agent.speed = baseSpeed;
    }
}
