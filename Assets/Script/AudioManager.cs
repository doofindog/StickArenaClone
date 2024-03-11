using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip themeSong;

    public void PlayOneShot(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    public void Play(AudioClip clip, float delay = 0.0f)
    {
        source.Stop();
        source.clip = clip;
        source.PlayDelayed(delay);
    }

    public void PlayThemeSong()
    {
        source.clip = themeSong;
        source.PlayDelayed(0.3f);
    }

    public void Stop()
    {
        source.Stop();
    }
    
}
