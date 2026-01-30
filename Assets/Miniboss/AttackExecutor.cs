using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AttackExecutor : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform target;

    private Coroutine dashRoutine;

    public void ExecuteAttack(SO_AttackData attack)
    {
        if (attack is SO_DashAttackData dashAttack)
            StartDash(dashAttack);
    }

    private void StartDash(SO_DashAttackData dashAttack)
    {
        if (dashRoutine != null)
            StopCoroutine(dashRoutine);

        dashRoutine = StartCoroutine(DashCoroutine(dashAttack));
    }

    private IEnumerator DashCoroutine(SO_DashAttackData dashAttack)
    {
        float originalSpeed = agent.speed;
        agent.speed *= dashAttack.speedMultiplier;

        float timer = 0f;
        while (timer < dashAttack.dashDuration)
        {
            agent.SetDestination(target.position);
            timer += Time.deltaTime;
            yield return null;
        }

        agent.speed = originalSpeed;
        dashRoutine = null;
    }
}
