using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class StickManData : NetworkBehaviour
{
    public const int NETWORK_BUFFER_SIZE = 1024;
    
    [SerializeField] private CharacterStatsScriptable _statsData;
    public float speed;
    public float aimAngle;
    public bool interactPressed;
    public bool attackPressed;
    public bool reloadPressed;
    public Vector2 direction;
    
    public NetStatePayLoad lastProcessedStatePayLoad;
    public NetStatePayLoad latestServerStatePayLoad;
    
    //ServerVariables;
    [SerializeField]private NetInputPayLoad[] _inputPayLoads;
    [SerializeField]private NetStatePayLoad[] _statePayLoads; 
    private Queue<NetInputPayLoad> _inputsQueue;

    
    public void Init()
    {
        speed = _statsData.speed;
        _inputPayLoads = new NetInputPayLoad[NETWORK_BUFFER_SIZE];
        _statePayLoads = new NetStatePayLoad[NETWORK_BUFFER_SIZE];
        _inputsQueue = new Queue<NetInputPayLoad>();
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

    public NetInputPayLoad GetCurrentInputPayLoad()
    {
        return new NetInputPayLoad()
        {
            tick = TickManager.Instance.GetTick(),
            direction = direction,
            aimAngle =  aimAngle
        };
    }

    public NetInputPayLoad[] GetInputPayload()
    {
        return _inputPayLoads;
    }

    public NetStatePayLoad[] GetStatePayLoads()
    {
        return _statePayLoads;
    }

    public NetStatePayLoad GetStatePayLoadAtIndex(int indexBuffer)
    {
        return _statePayLoads[indexBuffer];
    }

    public Queue<NetInputPayLoad> GetInputQueued()
    {
        return _inputsQueue;
    }
    
}
