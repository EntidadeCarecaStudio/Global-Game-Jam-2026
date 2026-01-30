using System.Collections;
using UnityEngine;

public abstract class SO_AttackData : ScriptableObject
{
    [Header("General")]
    public string attackName;

    [Header("Range")]
    public float minRange;
    public float maxRange;

    [Header("Cooldown")]
    public float cooldown;

    [Header("Context Requirements")]
    public float minCombatTime;
    public int minFailedAttempts;

    public bool IsOnCooldown { get; private set; } = false;

    public bool CanExecute(CombatContext context)
    {
        if (IsOnCooldown)
        {
            Debug.Log("Não vai rolar por conta do cooldown : " + IsOnCooldown);
            return false;
        }

        if (context.timeInCombat < minCombatTime)
        {
            Debug.Log("Não vai rolar porque ainda não tá na hora");
            return false;
        }

        if (context.failedAttackAttempts < minFailedAttempts)
        {
            Debug.Log("Não vai rolar porque ainda não errou o suficiente");
            return false;
        }

        if (context.currentDistance < minRange || context.currentDistance > maxRange)
        {
            Debug.Log("Não vai rolar porque a distância não bate");
            return false;
        }

        return true;
    }

    public void StartCooldown(MonoBehaviour owner)
    {
        Debug.Log(attackName + "Tá começando a contar o cooldown");
        if (cooldown <= 0)
            return;

        owner.StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        Debug.Log(IsOnCooldown + "Como isso é posssível?");
        IsOnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        IsOnCooldown = false;
    }

    public abstract void Execute(AttackContext context);
}
