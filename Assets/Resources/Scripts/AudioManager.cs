using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource sfxSource;  
    public AudioClip destroyClip;
    public AudioClip spawnClip;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayDestroySound()
    {
        if (sfxSource != null && destroyClip != null)
            sfxSource.PlayOneShot(destroyClip);
    }

    public void PlaySpawnSound()
    {
        if (sfxSource != null && spawnClip != null)
            sfxSource.PlayOneShot(spawnClip);
    }
}
