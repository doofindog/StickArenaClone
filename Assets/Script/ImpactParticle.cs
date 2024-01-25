using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactParticle : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void OnEnable()
    {
        if (particleSystem != null)
        {
            particleSystem.Play();
        }
    }

    void Update()
    {
        if (particleSystem.isStopped)
        {
            gameObject.SetActive(false);
        }
    }
}
