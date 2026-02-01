using UnityEngine;

public interface IMinibossMovement
{
    void StartMove();
    void StopMove();
    void ChaseMove(Vector3 targetPosition);
    bool IsInRange(Vector3 targetPosition, float range);
}
