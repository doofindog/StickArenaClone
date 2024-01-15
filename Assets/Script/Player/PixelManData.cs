using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PixelManData : NetworkBehaviour
{
    public const int NETWORK_BUFFER_SIZE = 1024;

    public enum State
    {
        Idle,
        Move,
        Dodge,
        Dead
    }

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
    
    [Header("State")] 
    public State state;
    public NetworkVariable<float> health = new NetworkVariable<float>();
    
    //ServerVariables;
    [SerializeField] private BasePixelManDataScriptable pixelManData;
    [SerializeField] private NetInputPayLoad[] _inputPayLoads;
    [SerializeField] private NetStatePayLoad[] _statePayLoads; 
    public NetStatePayLoad latestServerStatePayLoad;
    private Queue<NetInputPayLoad> _inputsQueue = new Queue<NetInputPayLoad>();
     
    public void Init()
    {
        if (IsServer)
        {
            health.Value = pixelManData.maxHealth;
            speed.Value = pixelManData.speed;
            dodgeSpeed.Value = pixelManData.dodgeSpeed;
            dodgeDuration.Value = pixelManData.dodgeDuration;
        } 

        _inputPayLoads = new NetInputPayLoad[NETWORK_BUFFER_SIZE];
        _statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
        _inputsQueue = new Queue<NetInputPayLoad>();
        canDodge = true;
        state = State.Idle;
    }
    

    [ServerRpc]
    public void SendInputServerRPC(NetInputPayLoad inputPayLoad)
    {
        _inputsQueue.Enqueue(inputPayLoad);
    }

    [ClientRpc]
    public void SendStateClientRPC(NetStatePayLoad statePayLoad)
    {
        latestServerStatePayLoad = statePayLoad;
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

    public NetInputPayLoad[] GetInputPayload()
    {
        return _inputPayLoads;
    }

    public NetInputPayLoad GetInputPayloadAtTick(int tick)
    {
        int index = tick % PixelManData.NETWORK_BUFFER_SIZE;
        return _inputPayLoads[index];
    }

    public NetStatePayLoad[] GetStatePayLoads()
    {
        return _statePayLoads;
    }

    public NetStatePayLoad GetStatePayLoadAtIndex(int indexBuffer)
    {
        return _statePayLoads[indexBuffer];
    }

    public NetStatePayLoad GetStatePayLoadAtTick(int tick)
    {
        int index = tick % PixelManData.NETWORK_BUFFER_SIZE;
        return _statePayLoads[index];
    }

    public Queue<NetInputPayLoad> GetInputQueued()
    {
        return _inputsQueue;
    }

    public void ReduceHealth(float reduceBy)
    {
        health.Value -= reduceBy;
    }
}
