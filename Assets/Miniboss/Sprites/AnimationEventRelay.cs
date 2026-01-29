using UnityEngine;

public class AnimationEventRelay : MonoBehaviour
{
    [SerializeField] private MinibossController controller;

    public void OnAnimationEvent(string eventName)
    {
        controller.OnAnimationEvent(eventName);
    }
}
