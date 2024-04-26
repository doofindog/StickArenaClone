using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    private float killTrigger;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float triggerThreshold = 3;
    [SerializeField] private AudioClip audioClip;

    public void Awake()
    {
        audioSource = audioSource.GetComponent<AudioSource>();
        GameEvents.PlayerDiedEvent += Cheer;
        GameEvents.OnGameStartEvent += OnGameStart;
    }

    private void OnGameStart()
    {
        audioSource.Play();
        AudioManager.Instance.PlayOneShot(audioClip);
    }

    private void Cheer(ulong clientID)
    {
        killTrigger++;
        if (killTrigger >= triggerThreshold)
        {
            AudioManager.Instance.PlayOneShot(audioClip);
            killTrigger = 0;
        }
    }
    
}
