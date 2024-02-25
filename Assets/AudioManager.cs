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

    public void PlayThemeSong()
    {
        source.clip = themeSong;
        source.PlayDelayed(0.3f);
    }
    
}
