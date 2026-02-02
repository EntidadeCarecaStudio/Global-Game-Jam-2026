using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // --- PARTE 1: SINGLETON (NOVO) ---
    // Garante que só existe um Audio Manager e que ele sobrevive à troca de cenas
    public static AudioManager Instance;

    // --- PARTE 2: DADOS E CONFIGURAÇÃO (MISTO) ---
    [Header("Configuração Geral")]
    public AudioMixer theMixer; // Mantivemos o nome original para não quebrar referências

    [Header("Biblioteca de Sons (NOVO)")]
    [SerializeField] private SoundLibrary soundLibrary; // Arraste o ScriptableObject aqui

    // Dicionário para busca rápida de sons
    private Dictionary<string, SoundData> soundMap = new Dictionary<string, SoundData>();

    // Adicione esta variável lá em cima junto com o soundMap
    private Dictionary<string, float> soundCooldowns = new Dictionary<string, float>();
    public float globalCooldownThreshold = 1f; // Tempo mínimo entre o mesmo som (0.1s)

    // --- PARTE 3: INICIALIZAÇÃO ---
    private void Awake()
    {
        // Configuração do Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // O segredo para a música não parar na troca de cena
        }
        else
        {
            // Se já existe um AudioManager vindo de outra cena, destrua este duplicado
            Destroy(gameObject);
            return;
        }

        // Prepara os sons da biblioteca
        InitializeSounds();
    }

    void Start()
    {
        // --- CÓDIGO DO OUTRO PROGRAMADOR (MANTIDO) ---
        // Mantemos a lógica de carregar os volumes salvos
        LoadVolumePreferences();
    }

    // --- PARTE 4: LÓGICA DO SISTEMA ---

    private void LoadVolumePreferences()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
            theMixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));

        if (PlayerPrefs.HasKey("MusicVol"))
            theMixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));

        if (PlayerPrefs.HasKey("SFXVol"))
            theMixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
    }

    private void InitializeSounds()
    {
        if (soundLibrary == null) return;

        foreach (SoundData s in soundLibrary.sounds)
        {
            GameObject soundObj = new GameObject("Sound_" + s.id);
            soundObj.transform.SetParent(this.transform);

            AudioSource source = soundObj.AddComponent<AudioSource>();

            // --- CONFIGURAÇÃO PADRÃO (ANTI-DISTORÇÃO) ---
            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = 1f;            // FORÇA velocidade normal
            source.loop = s.loop;
            source.spatialBlend = 0f;     // FORÇA som 2D (Crucial para músicas não ficarem "longe")
            source.playOnAwake = false;   // Garante que não toque sozinho

            // Se tiver o Mixer configurado, aplica aqui
            // source.outputAudioMixerGroup = ...; 

            s.source = source;

            if (!soundMap.ContainsKey(s.id))
            {
                soundMap.Add(s.id, s);
            }
        }
    }

    public void Play(string soundId)
    {
        if (soundMap.TryGetValue(soundId, out SoundData s))
        {
            // --- PROTEÇÃO ANTI-METRALHADORA ---
            // Verifica se este som específico tocou há muito pouco tempo
            if (soundCooldowns.ContainsKey(soundId))
            {
                float lastTime = soundCooldowns[soundId];
                if (Time.time - lastTime < globalCooldownThreshold)
                {
                    return; // Sai da função e ignora o pedido de tocar
                }
                soundCooldowns[soundId] = Time.time; // Atualiza o tempo
            }
            else
            {
                soundCooldowns.Add(soundId, Time.time); // Registra a primeira vez
            }
            // ----------------------------------

            // Reafirma o volume (útil para ajustes em tempo real)
            s.source.volume = s.volume;

            if (s.loop)
            {
                if (!s.source.isPlaying) s.source.Play();
            }
            else
            {
                s.source.PlayOneShot(s.clip);
            }
        }
        else
        {
            Debug.LogWarning($"AudioManager: Som '{soundId}' não encontrado!");
        }
    }

    public void Stop(string soundId)
    {
        if (soundMap.TryGetValue(soundId, out SoundData s))
        {
            s.source.Stop();
        }
    }
}