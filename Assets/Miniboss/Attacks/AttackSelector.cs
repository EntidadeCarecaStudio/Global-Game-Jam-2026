using System.Collections.Generic;
using UnityEngine;

public class AttackSelector : MonoBehaviour
{
    private readonly List<SO_AttackData> _attacks;

    public AttackSelector(List<SO_AttackData> attacks)
    {
        this._attacks = attacks;
    }

    public SO_AttackData SelectAttack(CombatContext context)
    {
        Debug.Log("Vai tentar selecionar um ataque");
        foreach (var attack in _attacks)
        {
            if (attack.CanExecute(context))
                return attack;
        }
        Debug.Log("Deu ruim ao tentar selecionar uma ataque");
        return null;
    }
}
