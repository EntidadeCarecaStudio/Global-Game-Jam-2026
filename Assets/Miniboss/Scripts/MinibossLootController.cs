using UnityEngine;

[RequireComponent(typeof(MinibossController))]
public class MinibossLootController : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("O prefab que será dropado ao morrer.")]
    [SerializeField] private GameObject _lootPrefab;

    [Tooltip("Ajuste de altura para o item não nascer dentro do chão.")]
    [SerializeField] private Vector3 _dropOffset = new Vector3(0, 0.5f, 0);

    private MinibossController _minibossController;
    private bool _hasDropped = false;

    private void Awake()
    {
        _minibossController = GetComponent<MinibossController>();
    }

    private void OnEnable()
    {
        // Inscreve-se no evento de morte (que criaremos no passo 3)
        if (_minibossController != null)
        {
            _minibossController.OnMinibossDeath += SpawnLoot;
        }
    }

    private void OnDisable()
    {
        // Boa prática: Desinscrever para evitar erros de memória
        if (_minibossController != null)
        {
            _minibossController.OnMinibossDeath -= SpawnLoot;
        }
    }

    private void SpawnLoot()
    {
        // Garante que só dropa uma vez
        if (_hasDropped) return;
        _hasDropped = true;

        if (_lootPrefab != null)
        {
            // Instancia o objeto na posição atual + offset, com rotação zerada
            // IMPORTANTE: O objeto é instanciado na cena, independente do Boss ser destruído depois
            Instantiate(_lootPrefab, transform.position + _dropOffset, Quaternion.identity);

            Debug.Log($"Loot '{_lootPrefab.name}' dropado por {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("Nenhum prefab de loot foi atribuído no Inspector!");
        }
    }
}