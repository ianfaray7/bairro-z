using UnityEngine;

/// <summary>
/// Gerencia músicas de background e SFX simples nas cenas Map_*.
/// - arraste os AudioClips (loop2, zumbi_hit, zumbi_death) no Inspector
/// - adicione esse componente em um GameObject na cena (ex: "MapAudioManager")
/// - marque "Play On Awake" se quiser iniciar música automaticamente
/// </summary>
public class MapAudioManager : MonoBehaviour
{
    public static MapAudioManager main;

    [Header("Music")]
    public AudioClip bgMusic;         // loop2.mp3
    [Range(0f, 1f)] public float musicVolume = 0.25f;

    [Header("SFX")]
    public AudioClip bowClip;         // bow.mp3
    public AudioClip zumbiHitClip;    // zumbi_hit.mp3
    public AudioClip zumbiDeathClip;  // zumbi_death.mp3

    [Header("SFX Master")]
    [Range(0f, 1f)] public float sfxMasterVolume = 0.9f;

    [Header("SFX per-sound")]
    [Range(0f, 1f)] public float bowVolume = 1f;
    [Range(0f, 1f)] public float zumbiHitVolume = 0.6f;
    [Range(0f, 1f)] public float zumbiDeathVolume = 0.6f;

    AudioSource musicSource;
    AudioSource sfxSource;

    void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }
        main = this;
        // cria fontes de audio
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        // keep source at unity; we'll multiply volumes on PlayOneShot to allow master/per-clip control
        sfxSource.volume = 1f;

        // toca música se atribuída
        if (bgMusic != null) PlayMusic(bgMusic);

        // Garantir que exista exatamente 1 AudioListener na cena
        EnsureSingleAudioListener();
    }

    void EnsureSingleAudioListener()
    {
        var listeners = FindObjectsOfType<AudioListener>(true);
        if (listeners == null || listeners.Length == 0)
        {
            // nenhum listener: adiciona um ao próprio GameObject (MapAudioManager)
            gameObject.AddComponent<AudioListener>();
            return;
        }

        // se houver mais de um, mantenha preferencialmente o que está no Camera.main
        if (listeners.Length > 1)
        {
            AudioListener primary = null;
            if (Camera.main != null)
                primary = Camera.main.GetComponent<AudioListener>();

            if (primary == null)
                primary = listeners[0];

            // desabilita os outros listeners
            foreach (var l in listeners)
            {
                if (l != primary)
                {
                    l.enabled = false;
                    Debug.LogWarning($"MapAudioManager: desabilitando AudioListener em '{l.gameObject.name}' para manter apenas um listener.", this);
                }
            }
        }
    }

    public void PlayMusic(AudioClip clip, float volume = -1f)
    {
        if (musicSource == null) return;
        musicSource.clip = clip;
        musicSource.volume = (volume < 0f) ? musicVolume : volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic() => musicSource?.Stop();

    public void PlaySFX(AudioClip clip, float vol = 1f)
    {
        if (clip == null || sfxSource == null) return;
        // vol = per-clip multiplier in [0..1]; sfxMasterVolume acts as overall SFX slider.
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(vol * sfxMasterVolume));
    }

    public void PlayBow() => PlaySFX(bowClip, bowVolume);
    public void PlayZombieHit() => PlaySFX(zumbiHitClip, zumbiHitVolume);
    public void PlayZombieDeath() => PlaySFX(zumbiDeathClip, zumbiDeathVolume);
}
