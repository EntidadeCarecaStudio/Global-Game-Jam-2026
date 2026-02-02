using UnityEngine;

public interface IMinibossMovement
{
    void StartMove(MovementContext context);
    void StopMove(MovementContext context);
    void Tick(MovementContext context);
    //bool IsInRange(MovementContext context, float range);
    /*
    public bool IsInRange(MovementContext context, float range)
    {
        return Vector3.Distance(context.Self.position, context.Target.position) <= range;
    }
    */
}
