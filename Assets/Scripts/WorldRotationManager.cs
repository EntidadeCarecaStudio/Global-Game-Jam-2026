using UnityEngine;
using System;

public class WorldRotationManager : MonoBehaviour
{
    
    public static event Action<float> OnRotateRequest;

    private void OnRotateClockwise()
    {
        OnRotateRequest?.Invoke(-90f);
    }

    private void OnRotateCounterclockwise()
    {
        OnRotateRequest?.Invoke(90f);
    }

    void OnEnable()
    {
        Manager_Events.Input.OnRotateClockwise += OnRotateClockwise;
        Manager_Events.Input.OnRotateCounterclockwise += OnRotateCounterclockwise;
    }

    void OnDisable()
    {
        Manager_Events.Input.OnRotateClockwise -= OnRotateClockwise;
        Manager_Events.Input.OnRotateCounterclockwise -= OnRotateCounterclockwise;
    }

}