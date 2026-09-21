using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip flipClip;
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip bgmClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
    }

    private void Start()
    {
        if (bgmClip != null && musicSource != null)
        {
            musicSource.clip = bgmClip;
            musicSource.Play();
        }
    }

    public void PlayFlip()
    {
        if (sfxSource != null && flipClip != null)
        {
            sfxSource.PlayOneShot(flipClip, 0.9f);
        }
    }

    public void PlayDeath()
    {
        if (sfxSource != null && deathClip != null)
        {
            sfxSource.PlayOneShot(deathClip, 1.0f);
        }
    }
}

