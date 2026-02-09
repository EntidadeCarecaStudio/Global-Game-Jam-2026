using UnityEngine;

public abstract class SO_AttackData : ScriptableObject
{
    [Header("General")]
    public string attackName;
    public AnimationClip attackAnimationClip;
    public float attackRadius;

    [Header("Range")]
    [Min(0f)] public float minRange;
    [Min(0.1f)] public float maxRange;

    [Header("Cooldown")]
    [Min(0f)] public float cooldown;
    [Min(0f)] public float recoveryTime;

    [Header("Context Requirements")]
    [Min(0f)] public float minCombatTime;
    [Min(0f)] public int minFailedAttempts;

    public bool IsValid(CombatContext context)
    {
        if (context.timeInCombat < minCombatTime)
            return false;

        if (context.failedAttackAttempts < minFailedAttempts)
            return false;

        if (context.currentDistance < minRange || context.currentDistance > maxRange)
            return false;

        return true;
    }

    public abstract void Execute(AttackContext context);
}
