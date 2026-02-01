using UnityEngine;
using UnityEngine.AI;

public class SpriteFlip : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        float x = _agent.desiredVelocity.x;

        if (Mathf.Abs(x) > 0.01f)
        {
            spriteRenderer.flipX = x < 0f;
        }
    }
}
