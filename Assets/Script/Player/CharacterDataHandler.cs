using System;
using Unity.Netcode;
using UnityEngine;

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
    public Vector3 direction;
    public Vector2 preDirection;
    public NetworkVariable<float> speed = new NetworkVariable<float>();

    [Header("Dodge")]
    public int dodgeTicksElapsed;
    public int totalDodgeTicks;
    public int cooldownTicksRemaining;
    public Vector3 dodgeStartPosition;
    public Vector3 dodgeTargetPosition;
    public AnticipatedNetworkVariable<bool> canDodge = new AnticipatedNetworkVariable<bool>();
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
    public NetworkVariable<int> health = new NetworkVariable<int>();
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>();
    
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
        
        canDodge.Anticipate(true);
        state = State.Idle;

        health.OnValueChanged = (value, newValue) =>
        {
            if (IsLocalPlayer)
            {
                PlayerEvents.SendPlayerDamageTake(this);
            }
        };
    }

    public void Reset()
    {
        if (IsServer)
        {
            maxHealth.Value = health.Value = pixelManData.maxHealth;
            speed.Value = pixelManData.speed;
        } 
        
        canDodge.Anticipate(true);
        state = State.Idle;
    }
    
    public float ReduceHealth(int reduceBy)
    {
        health.Value -= reduceBy;

        return health.Value;
    }
}
