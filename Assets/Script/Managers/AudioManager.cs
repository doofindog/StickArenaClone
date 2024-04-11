using UnityEngine;

public enum AudioChannel
{
    MUSIC,
    SFX
}

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip themeSong;

    public void PlayOneShot(AudioClip clip)
    {
        musicSource.PlayOneShot(clip);
    }
    
    public void Play(AudioClip clip, float volume = 0.0f, float delay = 0.0f)
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.PlayDelayed(delay);
    }

    public void SetVolume(AudioChannel channel, float volume)
    {
        volume = Mathf.Clamp01(volume);
        switch (channel)
        {
            case AudioChannel.MUSIC:
                musicSource.volume = volume;
                break;
            case AudioChannel.SFX:
                sfxSource.volume = volume;
                break;
        }
    }

    public float GetVolume(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.MUSIC:
                return musicSource.volume;
                break;
            case AudioChannel.SFX:
                return sfxSource.volume;
                break;
        }

        return 0;
    }

    public AudioSource GetSource()
    {
        return musicSource;
    }
    
    public void Stop()
    {
        musicSource.Stop();
    }
    
}
