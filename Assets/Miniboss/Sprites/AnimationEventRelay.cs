using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private MinibossController _controller;

    public void OnAttackFinished(string eventName)
    {
        _controller.ChangeState(_controller.AttackState);
    }
}
