using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    
    [SerializeField] private Animator _animator;

    private CharacterState m_currentAnimationState;

    public void UpdateAnimation(CharacterState newState)
    {
        if (m_currentAnimationState != newState)
        {
            m_currentAnimationState = newState;
            string animToPlay = newState.ToString();

            if (!string.IsNullOrEmpty(animToPlay))
            {
                _animator.Play(animToPlay);
            }
        }
    }

}