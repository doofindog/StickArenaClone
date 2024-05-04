using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlare : MonoBehaviour
{
    public ParticleSystem forwardFlare;
    public ParticleSystem sideFlare;

    public void Play()
    {
        forwardFlare.Play();
        sideFlare.Play();
    }
}
