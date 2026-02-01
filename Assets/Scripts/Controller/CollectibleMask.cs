using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleMask : MonoBehaviour, IInteractable
{
    [SerializeField] private MaskData _maskData;
    [SerializeField] private GameObject _interactionUI;
    [SerializeField] private bool _destroyOnCollect = true;

    private void Start()
    {
        if (_interactionUI != null)
        {
            _interactionUI.SetActive(false);
        }
    }

    public void Interact(GameObject interactor)
    {
        if (_maskData == null)
        {
            Debug.LogWarning("CollectibleMask on " + gameObject.name + " has no MaskData assigned.");
            return;
        }

        PlayerController playerController = interactor.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.EquipMask(_maskData);
            Debug.Log("Player equipped mask: " + _maskData.maskName);
            HideUI();
            if (_destroyOnCollect)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Interactor " + interactor.name + " is not a PlayerController. Cannot equip mask.");
        }
    }

    public void ShowUI()
    {
        if (_interactionUI != null)
        {
            _interactionUI.SetActive(true);
        }
    }

    public void HideUI()
    {
        if (_interactionUI != null)
        {
            _interactionUI.SetActive(false);
        }
    }

    private void OnValidate()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("Collider on " + gameObject.name + " was set to Is Trigger for CollectibleMask.");
        }
        else if (col == null)
        {
            Debug.LogWarning("CollectibleMask on " + gameObject.name + " requires a Collider component to function properly.");
        }
    }
}