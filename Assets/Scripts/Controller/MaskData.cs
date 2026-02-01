using UnityEngine;

[CreateAssetMenu(fileName = "NewMaskData", menuName = "Character/Mask Data")]
public class MaskData : ScriptableObject
{
    public string maskName = "Default Mask";
    public Sprite maskIcon;

    public int maxHealthModifier = 0;
    public int attackDamageModifier = 0;
    public float movementSpeedXModifier = 0f;
    public float movementSpeedZModifier = 0f;
    public float attackDurationModifier = 0f;
    public float dodgeDurationModifier = 0f;
    public float attackCooldownModifier = 0f;
    public float takeDamageStunDurationModifier = 0f;
    public float knockbackForceModifier = 0f;
    public float knockbackResistanceModifier = 0f;
}