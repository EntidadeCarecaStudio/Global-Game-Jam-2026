using UnityEngine;

[CreateAssetMenu(fileName = "SO_MeleeAttackData", menuName = "Scriptable Objects/Attacks/Melee Attack Data")]
public class SO_MeleeAttackData : SO_AttackData
{
    public override void Execute(AttackContext context)
    {
        if (context.agent.enabled)
            context.agent.isStopped = true;
    }
}
