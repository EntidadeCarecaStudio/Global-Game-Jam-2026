using UnityEngine;

public class InteractableTest : MonoBehaviour, IInteractable
{

    [SerializeField] private GameObject _canvas;

    void Awake()
    {
        HideUI();
    }

    public void HideUI()
    {
        _canvas.SetActive(false);
    }

    public void Interact(GameObject interactor)
    {
        Debug.Log($"Interact with {name} from {interactor.name}");
    }

    public void ShowUI()
    {
        _canvas.SetActive(true);
    }

}
