using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class optionUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;

    public void OnEnable()
    {
        musicSlider.value = Mathf.Clamp01(AudioManager.Instance.GetVolume(AudioChannel.MUSIC));
    }

    public void OnMusicValueChane()
    {
        AudioManager.Instance.SetVolume(AudioChannel.MUSIC, musicSlider.value);
    }
}
