using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Necessário para Corrotinas
using static Manager_Events;
using Input = UnityEngine.Input;

public class Pause : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _pauseMenuUI;

    [Header("Animation Settings")]
    [SerializeField] private float _animationDuration = 0.4f;

    // Curva: 0 -> 1.15 -> 0.95 -> 1.0
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

        if (_musicSource != null)
            _originalVolume = _musicSource.volume;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Impede spam do botão enquanto estiver animando (opcional, mas recomendado)
            // if (_animationCoroutine != null) return; 

            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        _isPaused = true;

        // 1. Pausa lógica do jogo imediatamente
        Time.timeScale = 0f;

        // 2. Ajusta som
        if (_musicSource != null)
        {
            _originalVolume = _musicSource.volume; // Salva o volume atual
            _musicSource.volume = _pausedVolume;
        }

        // 3. Ativa e Anima (ABRIR = true)
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

        // NOTA: Não mudamos Time.timeScale para 1 aqui ainda!
        // O jogo continua pausado visualmente enquanto o menu encolhe.

        // 1. Inicia Animação de Fechar (ABRIR = false)
        if (_pauseMenuUI != null)
        {
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(AnimateMenu(false));
        }
        else
        {
            // Fallback caso não tenha UI linkada
            FinishResume();
        }
    }

    // Esta função é chamada automaticamente quando a animação de fechar termina
    private void FinishResume()
    {
        // Desativa a UI
        if (_pauseMenuUI != null)
            _pauseMenuUI.SetActive(false);

        // AGORA sim voltamos o jogo ao normal
        Time.timeScale = 1f;

        // Restaura som
        if (_musicSource != null)
            _musicSource.volume = _originalVolume;

        // Opcional: Travar mouse novamente
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }

    // --- CORROTINA UNIFICADA (ABRIR E FECHAR) ---
    private IEnumerator AnimateMenu(bool isOpening)
    {
        float timer = 0f;

        while (timer < _animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / _animationDuration;

            // Se estiver abrindo: vai de 0 a 1 na curva
            // Se estiver fechando: vai de 1 a 0 na curva
            float curveTime = isOpening ? progress : (1f - progress);

            float scaleValue = _popCurve.Evaluate(curveTime);

            _pauseMenuUI.transform.localScale = Vector3.one * scaleValue;

            yield return null;
        }

        // Finalização forçada para evitar erros de float
        if (isOpening)
        {
            _pauseMenuUI.transform.localScale = Vector3.one;
        }
        else
        {
            _pauseMenuUI.transform.localScale = Vector3.zero;
            // Chama a função que realmente "despausa" o jogo
            FinishResume();
        }

        _animationCoroutine = null;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_mainMenuSceneName);
    }
}
