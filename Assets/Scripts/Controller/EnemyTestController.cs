using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyTestController : MonoBehaviour, IDamageable
{

    [SerializeField] private CharacterStats _stats;
    [SerializeField] private CharacterAnimationController _animatorController;
    [SerializeField] private Slider _slider;
    [SerializeField] private float _takeDamageWaiter = 0.5f;
    [SerializeField] private float _dieWaiter = 0.5f;

    private int m_currentHp;
    public int CurrentHp => m_currentHp;

    void Awake()
    {
        m_currentHp = _stats.maxHealth;

        UpdateSlider();
    }

    public void TakeDamage(int damage)
    {
        m_currentHp -= damage;

        UpdateSlider();

        if (m_currentHp <= 0)
        {
            _animatorController.UpdateAnimation(CharacterState.Die);
            StartCoroutine(WaitDie());
        }
        else
        {
            _animatorController.UpdateAnimation(CharacterState.TakeDamage);
            StartCoroutine(WaitTakeDamage());
        }
    }

    private IEnumerator WaitTakeDamage()
    {
        yield return new WaitForSeconds(_takeDamageWaiter);

        _animatorController.UpdateAnimation(CharacterState.Idle);
    }

    private IEnumerator WaitDie()
    {
        yield return new WaitForSeconds(_dieWaiter);

        Destroy(gameObject);
    }

    private void UpdateSlider()
    {
        _slider.SetValueWithoutNotify((float) m_currentHp / _stats.maxHealth);
    }

}
