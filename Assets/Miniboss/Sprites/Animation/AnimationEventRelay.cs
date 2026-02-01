using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private MinibossController _controller;

    public void OnAnimationEvent(string eventName)
    {
        _controller.OnAnimationEvent(eventName);
    }
}
