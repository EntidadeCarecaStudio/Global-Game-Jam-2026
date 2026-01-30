using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor);
    void ShowUI();
    void HideUI();
}