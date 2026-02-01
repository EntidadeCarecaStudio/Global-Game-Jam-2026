using UnityEngine;

[System.Serializable]
public class SoundData
{
    public string id;             // Ex: "BGM_Fase1"
    public AudioClip clip;        // O arquivo de áudio

    [Range(0f, 1f)]
    public float volume = 1f;     // Única coisa que você quer mexer

    public bool loop;             // Se é música ou ambiente, marque isso

    [HideInInspector]
    public AudioSource source;    // Uso interno do sistema
}

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundData[] sounds;
}