using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

public class CharacterAnimator : NetworkAnimator
{
    private const string DEATH_KEY = "death";
    private const string DAMAGE_KEY = "damage";
    
    private static readonly int Death = Animator.StringToHash(DEATH_KEY);
    private static readonly int Damage = Animator.StringToHash(DAMAGE_KEY);

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

    public void PlayIdle()
    {
        SetTrigger("idle");
    }

    public void PlayTakeDamage(bool isSync)
    {
        if (isSync)
        {
            SetTrigger("damage");
            return;
        }
        
        Animator.SetTrigger(Damage);
    }

    public void DeathAnimation(bool isSync)
    {
        if (isSync)
        {
            SetTrigger("death");
            return;
        }
        
        Animator.SetTrigger(Death);
    }
} 
