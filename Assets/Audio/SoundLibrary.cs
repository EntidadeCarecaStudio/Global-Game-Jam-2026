using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundData
{
    public string id;             // O nome que o programador vai usar (ex: "PlayerJump")
    public AudioClip clip;        // O ficheiro de áudio real
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop;

    [HideInInspector] public AudioSource source; // Usado internamente pelo Manager
}

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public SoundData[] sounds;
}