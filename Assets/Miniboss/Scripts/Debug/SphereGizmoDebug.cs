using UnityEngine;

public class SphereGizmoDebug : MonoBehaviour
{
    public float radius = 0.5f;
    [Range(0f, 1f)]
    public float alpha = 0.3f;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, alpha);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
