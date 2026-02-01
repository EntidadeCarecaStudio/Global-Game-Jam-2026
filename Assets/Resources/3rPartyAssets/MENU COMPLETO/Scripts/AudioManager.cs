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

        // Cria um AudioSource para cada som configurado na Library
        foreach (SoundData s in soundLibrary.sounds)
        {
            // Cria um objeto filho para organizar a hierarquia
            GameObject soundObj = new GameObject("Sound_" + s.id);
            soundObj.transform.SetParent(this.transform);

            AudioSource source = soundObj.AddComponent<AudioSource>();

            // Configura o source com os dados do Scriptable Object
            source.clip = s.clip;
            source.volume = s.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;

            // Opcional: Roteia para o Mixer Group correto (Music ou SFX) se desejar
            // source.outputAudioMixerGroup = ... (Isso pode ser adicionado no SoundData depois)

            s.source = source; // Guarda a referência no objeto de dados runtime

            if (!soundMap.ContainsKey(s.id))
            {
                soundMap.Add(s.id, s);
            }
        }
    }

    // --- PARTE 5: MÉTODOS PÚBLICOS (API PARA OS PROGRAMADORES) ---

    public void Play(string soundId)
    {
        if (soundMap.TryGetValue(soundId, out SoundData s))
        {
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
            Debug.LogWarning($"AudioManager: Som '{soundId}' não encontrado na Library!");
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