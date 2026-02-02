using UnityEngine;

public enum AnimationEventType
{
    AttackEnds,
    HitDetection,
    PlayVFX
}

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private MinibossController _controller;

    public void OnAnimationEvent(AnimationEventType eventType)
    {
        _controller.OnAnimationEvent(eventType);
    }
}
