using UnityEngine;

public class AudioEventListener : MonoBehaviour
{
    private void OnEnable()
    {
        // ASSINAR: "Quando o PlayerController gritar 'OnPlayerDodge', execute 'PlayDashSound'"
        PlayerController.OnPlayerDodge += PlayDashSound;
        PlayerController.OnPlayerAttack += PlayAttackSound;
        PlayerController.OnPlayerGetHit += PlayHitSound;
        PlayerController.OnPlayerGetKilled += PlayKillSound;
    }

    private void OnDisable()
    {
        // CANCELAR ASSINATURA: Muito importante para evitar erros de memória ao trocar de cena
        PlayerController.OnPlayerDodge -= PlayDashSound;
        PlayerController.OnPlayerAttack -= PlayAttackSound;
        PlayerController.OnPlayerGetHit -= PlayHitSound;
        PlayerController.OnPlayerGetKilled -= PlayKillSound;
    }

    // A AÇÃO
    private void PlayDashSound()
    {
        // Aqui chamamos o AudioManager (com aquele sistema de proteção que criamos antes)
        // Certifique-se que "Dash" é o ID exato que está no seu Scriptable Object
        AudioManager.Instance.Play("_golpe2");
    }

    private void PlayAttackSound()
    {
        // Aqui chamamos o AudioManager (com aquele sistema de proteção que criamos antes)
        // Certifique-se que "Dash" é o ID exato que está no seu Scriptable Object
        AudioManager.Instance.Play("_golpe1");
    }

    private void PlayHitSound()
    {
        // Aqui chamamos o AudioManager (com aquele sistema de proteção que criamos antes)
        // Certifique-se que "Dash" é o ID exato que está no seu Scriptable Object
        AudioManager.Instance.Play("_scream1");
    }

    private void PlayKillSound()
    {
        // Aqui chamamos o AudioManager (com aquele sistema de proteção que criamos antes)
        // Certifique-se que "Dash" é o ID exato que está no seu Scriptable Object
        AudioManager.Instance.Play("_scream2");
        AudioManager.Instance.Play("_death1");
    }

}