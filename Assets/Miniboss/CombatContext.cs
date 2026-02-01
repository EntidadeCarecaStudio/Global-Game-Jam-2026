[System.Serializable]
public class CombatContext
{
    public float timeInCombat;
    public float timeSinceLastAttack;
    public float timeSincePlayerInRange;
    public int failedAttackAttempts;

    public float currentDistance;

    public void ResetAttackTimers()
    {
        timeSinceLastAttack = 0f;
        failedAttackAttempts = 0;
    }

    public void RegisterFailedAttack()
    {
        failedAttackAttempts++;
    }
}
