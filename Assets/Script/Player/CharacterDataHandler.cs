using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterDataHandler : NetworkBehaviour
{
    public enum State
    {
        Idle,
        Move,
        Dodge,
        Dead
    }
    public const int NETWORK_BUFFER_SIZE = 1024;
    
    [SerializeField] private BasePixelManDataScriptable pixelManData;

    [Header("Movement")]
    public NetworkVariable<float> speed = new NetworkVariable<float>();
    public Vector2 direction;

    [Header("Dodge")] public bool canDodge;
    public NetworkVariable<float> dodgeDuration = new NetworkVariable<float>();
    public NetworkVariable<float> dodgeSpeed = new NetworkVariable<float>();

    [Header("Aim")]
    public float aimAngle;
    
    [Header("Interactions")]
    public bool dodgePressed;
    public bool interactPressed;
    public bool attackPressed;
    public bool reloadPressed;
    public bool swapPressed;
    
    [Header("Health")] 
    public NetworkVariable<float> health = new NetworkVariable<float>();
    public NetworkVariable<float> maxHealth = new NetworkVariable<float>();
    
    public State state;
    
     
    public void Init()
    {
        if (IsServer)
        {
            maxHealth.Value = health.Value = pixelManData.maxHealth;
            speed.Value = pixelManData.speed;
            dodgeSpeed.Value = pixelManData.dodgeSpeed;
            dodgeDuration.Value = pixelManData.dodgeDuration;
        } 
        
        canDodge = true;
        state = State.Idle;

        health.OnValueChanged = (value, newValue) =>
        {
            if (IsLocalPlayer)
            {
                PlayerEvents.SendPlayerDamageTake(this, MathF.Abs(value - newValue));
            }
        };
    }

    public NetInputPayLoad GetNewInputPayLoad()
    {
        return new NetInputPayLoad()
        {
            tick = TickManager.Instance.GetTick(),
            direction = direction,
            aimAngle =  aimAngle,
            dodgePressed = dodgePressed
        };
    }
    
    public void ReduceHealth(float reduceBy)
    {
        Debug.Log("called On value changed");
        health.Value -= reduceBy;
    }
}
