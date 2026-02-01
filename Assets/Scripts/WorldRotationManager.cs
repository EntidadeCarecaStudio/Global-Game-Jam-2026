using UnityEngine;
using System;

public class WorldRotationManager : MonoBehaviour
{
    // Evento que o Player e a Câmera (se necessário) escutarão
    public static event Action<float> OnRotateRequest;

    void Update()
    {
        // Detecta o input e dispara o evento para quem estiver ouvindo (o Player)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnRotateRequest?.Invoke(90f); // Gira 90 graus para um lado
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnRotateRequest?.Invoke(-90f); // Gira 90 graus para o outro
        }
    }
}