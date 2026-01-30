using UnityEngine;

[CreateAssetMenu(fileName = "MinibossStats", menuName = "Scriptable Objects/Miniboss Stats")]
public class SO_MinibossStats : ScriptableObject
{
    [Min(1f)] public float maxHealth = 100f;
    [Min(0f)] public float force = 5f;
    [Min(1f)] public float defense = 5f;
    [Min(0.1f)] public float moveSpeed = 5f;
    [Min(0.1f)] public float dashSpeed = 10f;
    [Min(0.1f)] public float moveAcceleration = 8f;
    [Range(0f, 1f)] public float criticalChance = 0.08f;
    [Min(1f)] public float criticalMultiplier = 3f;
    [Min(0.1f)] public float attackEnterRange = 2f;
}
