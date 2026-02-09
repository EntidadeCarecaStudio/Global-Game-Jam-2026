using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Character/Character Stats")]
public class CharacterStats : ScriptableObject
{
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float movementSpeedX = 5.0f;
    public float movementSpeedZ = 3.0f;
    public float attackDuration = 0.4f;
    public float dodgeDuration = 0.5f;
    public float attackCooldown = 1.0f;
    public float takeDamageStunDuration = 0.5f;
    public float damageFeedbackDuration = 0.1f;
    public float knockbackForce = 10.0f;
    [Range(0f, 1f)]
    public float knockbackResistance = 0f;
}