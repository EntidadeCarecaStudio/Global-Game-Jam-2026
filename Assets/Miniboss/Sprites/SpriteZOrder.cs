using UnityEngine;

public class SpriteZOrder : MonoBehaviour
{
    [SerializeField] private Transform feetTransform;
    private SpriteRenderer sr;
    private float lastZ;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        float z = feetTransform.position.z;
        if (Mathf.Approximately(z, lastZ)) return;

        lastZ = z;
        sr.sortingOrder = Mathf.RoundToInt(-z * 100);
    }
}
