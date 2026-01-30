// MaskData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewMaskData", menuName = "Character/Mask Data")]
public class MaskData : ScriptableObject
{
    public string maskName = "Default Mask";
    public Sprite maskIcon; // Para uso futuro em UI, talvez.

    [Header("Stat Modifiers")]
    public int attackDamageModifier = 0;
    public float movementSpeedXModifier = 0f;
    public float movementSpeedZModifier = 0f;
    public float attackDurationModifier = 0f; // Ex: -0.1f para atacar 0.1s mais rápido
    public float dodgeDurationModifier = 0f;  // Ex: -0.1f para esquivar 0.1s mais rápido
    public float attackCooldownModifier = 0f; // Ex: -0.2f para reduzir cooldown
    public float takeDamageStunDurationModifier = 0f; // Ex: -0.1f para reduzir stun time
    public float knockbackForceModifier = 0f;         // Ex: +5f para aumentar o knockback que aplica
    public float knockbackResistanceModifier = 0f;    // Ex: +0.2f para reduzir o efeito do knockback recebido (0 a 1)

    // Adicione mais modificadores conforme necessário para outros stats do CharacterStats.
}