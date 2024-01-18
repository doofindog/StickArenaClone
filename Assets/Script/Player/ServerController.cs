using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerController : NetController, ITickableEntity, IDamageableEntity
{
    private NetInputProcessor _netInputProcessor;
    private NetStateProcessor _netStateProcessor;

    public override void Awake()
    {
        base.Awake();

        _netInputProcessor = GetComponent<NetInputProcessor>();
        _netStateProcessor = GetComponent<NetStateProcessor>();

        _netInputProcessor.processedInputEvent += HandleInputProcessed;
        _netInputProcessor.processCompletedEvent += HandleInputProcessedCompleted;
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer && !IsHost)
        {
            Destroy(this);
        }
		
        TickManager.Instance.AddEntity(this);
    }


    public void UpdateTick(int tick)
    {
        _netInputProcessor.ProcessInputs();
    }

    private void HandleInputProcessed(NetInputPayLoad inputPayLoad)
    {
        if (!IsOwner)
        {
            ProcessMovement(inputPayLoad);
        }

        NetStatePayLoad statePayLoad = new NetStatePayLoad()
        {
            tick = inputPayLoad.tick,
            position = transform.position,
            aimAngle = inputPayLoad.aimAngle,
            dodge = inputPayLoad.dodgePressed
        };
        
        _netStateProcessor.AddState(statePayLoad);
        _netStateProcessor.UpdateLastProcessedState(statePayLoad);
    }

    private void HandleInputProcessedCompleted()
    {
        _netStateProcessor.SendStateClientRpc(_netStateProcessor.GetLastProcessedState());
    }

    public override void TakeDamage(float damage)
    {
        if (IsServer)
        {
            dataHandler.ReduceHealth(damage);
        }
    }

    public override void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }
}
