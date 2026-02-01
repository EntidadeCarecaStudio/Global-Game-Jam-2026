using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackExecutor : MonoBehaviour
{
    private Dictionary<SO_AttackData, bool> cooldowns = new Dictionary<SO_AttackData, bool>();

    private bool isInRecovery;

    public bool IsBusy => isInRecovery;

    public bool CanExecute(SO_AttackData attack)
    {
        if (isInRecovery) return false;
        if (cooldowns.TryGetValue(attack, out bool onCooldown))
            return !onCooldown;

        return true;
    }

    public bool ExecuteAttack(SO_AttackData attack, AttackContext context)
    {
        if (!CanExecute(attack)) return false;

        if (attack.cooldown > 0)
            cooldowns[attack] = true;

        attack.Execute(context);

        if (attack.cooldown > 0)
            StartCoroutine(CooldownRoutine(attack, attack.cooldown));

        if (attack.recoveryTime > 0)
            StartCoroutine(RecoveryRoutine(attack.recoveryTime));

        return true;
    }

    private IEnumerator CooldownRoutine(SO_AttackData attack, float time)
    {
        cooldowns[attack] = true;
        yield return new WaitForSeconds(time);
        cooldowns[attack] = false;
    }

    private IEnumerator RecoveryRoutine(float time)
    {
        isInRecovery = true;
        yield return new WaitForSeconds(time);
        isInRecovery = false;
    }
}
