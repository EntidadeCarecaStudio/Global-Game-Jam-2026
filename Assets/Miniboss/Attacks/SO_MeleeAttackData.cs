using UnityEngine;

[CreateAssetMenu(fileName = "SO_MeleeAttackData", menuName = "Scriptable Objects/Attacks/Melee Attack Data")]
public class SO_MeleeAttackData : SO_AttackData
{
    public override void Execute(AttackContext context)
    {
        context.agent.isStopped = true;

        context.attacker.GetComponent<MinibossController>().Animator.PlayMeleeAttack();
        // Executar alguma lógica de dano
    }
}
