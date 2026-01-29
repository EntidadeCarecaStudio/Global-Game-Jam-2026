using System;
using UnityEngine;

public class MinibossProximitySensor : MonoBehaviour
{
    public event Action<Transform> OnTargetEnterRange;
    public event Action OnTargetExitRange;

    [SerializeField] private LayerMask targetLayer;

    private bool isTargetInRange = false;
    public bool IsTargetInRange => isTargetInRange;

    private void Awake()
    {
        var collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsTarget(other))
            return;

        isTargetInRange = true;
        OnTargetEnterRange?.Invoke(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsTarget(other))
            return;

        isTargetInRange = false;
    }

    private bool IsTarget(Collider other)
    {
        return ((1 << other.gameObject.layer) & targetLayer) != 0;
    }
}
