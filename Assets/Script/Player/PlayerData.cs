using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public enum State
    {
        Idle,
        Move,
        Dodge,
        Dead
    }
    
    [SerializeField] private PlayerSettings PlayerSettings;

    [Header("Movement")]
    public Vector3 direction;
    public Vector2 preDirection;
    public NetworkVariable<float> speed = new NetworkVariable<float>();

    [Header("Aim")]
    public float aimAngle;
    
    [Header("Interactions")]
    public bool dodgePressed;
    public bool interactPressed;
    public bool shootPressed;
    public bool reloadPressed;
    public bool swapPressed;
    
    [Header("Health")] 
    public NetworkVariable<int> health = new NetworkVariable<int>();
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>();
    
    public State state;
    
    public void Init()
    {
        void HandleServer()
        {
            health.Value = PlayerSettings.maxHealth;
            speed.Value = PlayerSettings.speed;
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, null);
    }

    public void Reset()
    {
        if (IsServer)
        {
            maxHealth.Value = health.Value = PlayerSettings.maxHealth;
            speed.Value = PlayerSettings.speed;
        } 

        state = State.Idle;
    }
}
