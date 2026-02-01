using System.Collections.Generic;
using UnityEngine;

public class AttackSelector : MonoBehaviour
{
    [Header("Attacks")]
    [SerializeField] private List<SO_AttackData> _attacks;

    public SO_AttackData SelectAttack(CombatContext context, AttackExecutor executor)
    {
        foreach (var attack in _attacks)
        {
            if (!attack.IsValid(context))
                continue;

            if (!executor.CanExecute(attack))
                continue;

            return attack;
        }
        return null;
    }
}
