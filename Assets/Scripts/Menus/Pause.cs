using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using static Manager_Events;
using Input = UnityEngine.Input;

public class Pause : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenuUI;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.4f;

    [SerializeField]
    private AnimationCurve _popCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1.15f),
        new Keyframe(0.8f, 0.95f),
        new Keyframe(1f, 1f)
    );

    [Header("Audio Settings")]
    [SerializeField] private AudioSource _musicSource;
    [Range(0f, 1f)][SerializeField] private float _pausedVolume = 0.3f;

    private float _originalVolume;
    private bool _isPaused = false;
    private Coroutine _animationCoroutine;

    [Header("Scene Management")]
    [SerializeField] private string _mainMenuSceneName = "MainMenu";

    private void Start()
    {
        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(false);
            _pauseMenuUI.transform.localScale = Vector3.zero;
        }

        // Salva o volume inicial apenas uma vez no começo
        if (_musicSource != null)
            _originalVolume = _musicSource.volume;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;

        // 1. Pausa o jogo
        Time.timeScale = 0f;

        // 2. Não setamos o volume aqui abruptamente mais.
        // A corrotina vai cuidar disso suavemente.

        // 3. Inicia Animação de Abrir
        if (_pauseMenuUI != null)
        {
            _pauseMenuUI.SetActive(true);

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AnimateMenu(true));
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        _isPaused = false;

        // 1. Inicia Animação de Fechar
        // O Time.timeScale continua 0 até a animação acabar (no FinishResume)
        if (_pauseMenuUI != null)
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AnimateMenu(false));
        }
        else
        {
            FinishResume();
        }
    }

    private void FinishResume()
    {
        if (_pauseMenuUI != null)
            _pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;

        // Garante que o volume fique exatamente no original no final (para corrigir pequenos erros de cálculo)
        if (_musicSource != null)
            _musicSource.volume = _originalVolume;

        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    // --- CORROTINA (AGORA CONTROLA UI E ÁUDIO) ---
    private IEnumerator AnimateMenu(bool isOpening)
    {
        float timer = 0f;

        // Configurações de AUDIO para a transição
        float startVol = (_musicSource != null) ? _musicSource.volume : 0f;
        float targetVol = isOpening ? _pausedVolume : _originalVolume;

        while (timer < _animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / _animationDuration);

            // --- 1. Lógica da UI (Curva) ---
            float curveTime = isOpening ? progress : (1f - progress);
            float scaleValue = _popCurve.Evaluate(curveTime);
            _pauseMenuUI.transform.localScale = Vector3.one * scaleValue;

            // --- 2. Lógica do Áudio (Lerp Linear) ---
            if (_musicSource != null)
            {
                // Mathf.Lerp faz a transição suave entre o volume inicial e o final
                _musicSource.volume = Mathf.Lerp(startVol, targetVol, progress);
            }

            yield return null;
        }

        // Finalização
        if (isOpening)
        {
            _pauseMenuUI.transform.localScale = Vector3.one;
            // Garante volume final correto
            if (_musicSource != null) _musicSource.volume = _pausedVolume;
        }
        else
        {
            _pauseMenuUI.transform.localScale = Vector3.zero;
            // O volume será cravado no _originalVolume dentro de FinishResume()
            FinishResume();
        }

        _animationCoroutine = null;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        if (_musicSource != null) _musicSource.volume = _originalVolume; // Reseta volume ao reiniciar
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        if (_musicSource != null) _musicSource.volume = _originalVolume; // Reseta volume ao sair
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
