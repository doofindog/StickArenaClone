using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class CharacterAnimator : NetworkAnimator
{
    [SerializeField] private Animator anim;
    
    private const string DEATH_KEY = "death";
    private const string DAMAGE_KEY = "damage";
    private const string DROWN_KEY = "drown";
    
    private static readonly int Death = Animator.StringToHash(DEATH_KEY);
    private static readonly int Damage = Animator.StringToHash(DAMAGE_KEY);
    private static readonly int Drown = Animator.StringToHash(DROWN_KEY);

    public void Start()
    {
        Animator = GetComponent<Animator>();
    }

    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }

    public void PlayWalk()
    {
        SetTrigger("walk");
    }

    public void PlayIdle(bool isSync)
    {
        if (isSync)
        {
            SetTrigger("idle");
            return;
        }

        Animator.Play("Idle");
    }

    public void PlayTakeDamage(bool isSync)
    {
        if (isSync)
        {
            SetTrigger(Damage);
            return;
        }
        
        anim.SetTrigger(Damage);
    }

    public void PlayDeathAnimation(bool isSync)
    {
        if (isSync)
        {
            SetTrigger(Death);
            return;
        }
        
        Animator.SetTrigger(Death);
    }

    public void PlayDrownAnimation(bool isSync)
    {
        if (isSync)
        {
            SetTrigger(Drown);
            return;
        }
        
        Animator.SetTrigger(Drown);
    }
} 
