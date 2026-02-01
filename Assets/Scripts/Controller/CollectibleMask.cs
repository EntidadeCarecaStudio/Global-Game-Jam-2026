using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CollectibleMask : MonoBehaviour, IInteractable
{
    [SerializeField] private MaskData _maskData;
    [SerializeField] private GameObject _interactionUI;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private RectTransform _ctnMask;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private bool _destroyOnCollect = true;

    private void Start()
    {
        if (_interactionUI != null)
        {
            _interactionUI.SetActive(false);
        }

        if (_maskData != null)
        {
            _sprite.sprite = _maskData.maskIcon;

            StringBuilder sb = new();

            sb.Append($"{_maskData.maskName} Mask\n");

            if (_maskData.maxHealthModifier != 0) sb.AppendLine($"\nHP {(_maskData.maxHealthModifier > 0 ? "+" : "-")} {_maskData.maxHealthModifier}");
            if (_maskData.attackDamageModifier != 0) sb.AppendLine($"\nATK {(_maskData.attackDamageModifier > 0 ? "+" : "-")} {_maskData.attackDamageModifier}");
            if (_maskData.movementSpeedModifier != 0f) sb.AppendLine($"\nSPD {(_maskData.movementSpeedModifier > 0 ? "+" : "-")} {_maskData.movementSpeedModifier}%");
            if (_maskData.attackDurationModifier != 0f) sb.AppendLine($"\nATK T. {(_maskData.attackDurationModifier > 0 ? "+" : "-")} {_maskData.attackDurationModifier}");
            if (_maskData.dodgeDurationModifier != 0f) sb.AppendLine($"\nDES T. {(_maskData.dodgeDurationModifier > 0 ? "+" : "-")} {_maskData.dodgeDurationModifier}");
            if (_maskData.attackCooldownModifier != 0f) sb.AppendLine($"\nATK C. {(_maskData.attackCooldownModifier > 0 ? "+" : "-")} {_maskData.attackCooldownModifier}");
            if (_maskData.takeDamageStunDurationModifier != 0f) sb.AppendLine($"\nDMG S. {(_maskData.takeDamageStunDurationModifier > 0 ? "+" : "-")} {_maskData.takeDamageStunDurationModifier}");
            if (_maskData.knockbackForceModifier != 0f) sb.AppendLine($"\nKNB F. {(_maskData.knockbackForceModifier > 0 ? "+" : "-")} {_maskData.knockbackForceModifier}");
            if (_maskData.knockbackResistanceModifier != 0f) sb.AppendLine($"\nKNB R. {(_maskData.knockbackResistanceModifier > 0 ? "+" : "-")} {_maskData.knockbackResistanceModifier}");

            _ctnMask.sizeDelta = new(1.5f, 0.2f * (sb.ToString().Count(x => x == '\n') + 1));

            _text.SetText(sb.ToString());
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