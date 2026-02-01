using UnityEngine;

[CreateAssetMenu(fileName = "MinibossStats", menuName = "Scriptable Objects/Miniboss/Stats")]
public class SO_MinibossStats : ScriptableObject
{
    //[Header("Basic")]
    //[Min(1f)] public float health = 100f;
    //[Min(0f)] public float force = 2f;
    //[Min(1f)] public float defense = 3f;

    [Header("Movement")]
    //[Min(0.1f)] public float moveSpeed = 1f;
    //[Min(0.1f)] public float dashSpeed = 3f;
    [Min(0.1f)] public float moveAcceleration = 8f;
    [Min(0.1f)] public float stopDistance = 1f;
    [Min(0.1f)] public float chaseRange = 12f;

    [Header("Combat")]
    [Min(0.1f)] public float attackRange = 3f;
    [Range(0f, 1f)] public float criticalChance = 0.08f;
    [Min(1f)] public float criticalMultiplier = 3f;
}
