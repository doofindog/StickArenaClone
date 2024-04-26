using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
        base.OnNetworkSpawn();
        if (!IsServer && !IsHost)
        {
            Destroy(this);
        }
		
        TickManager.Instance.AddEntity(this);
    }


    public void UpdateTick(int tick)
    {
        _netInputProcessor.ProcessInputs();
        _netStateProcessor.AddState(_netStateProcessor.GetLastProcessedState());
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
            dodge = inputPayLoad.dodgePressed,
            firedWeapon =  inputPayLoad.attackPressed
        };
        
        _netStateProcessor.UpdateLastProcessedState(statePayLoad);
    }

    private void HandleInputProcessedCompleted()
    {
        _netStateProcessor.SendStateClientRpc(_netStateProcessor.GetLastProcessedState());
    }

    public override void TakeDamage(HitResponseData hitResponseData)
    {
        // Server Side Rewind
        // ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(hitResponseData.sourceID);
        // float rewindTime = Time.realtimeSinceStartup * 1000 - ping;
        // int rewindTick = Convert.ToInt32((rewindTime * (TickManager.Instance.GetTick() % 1024)) / (Time.realtimeSinceStartup * 1000));
        //
        // NetStatePayLoad hitState = _netStateProcessor.GetStateAtTick(rewindTick);
        //
        // Debugger.Log("[SERVER] ping : "+ ping +", rewardTime :" + rewindTime + ", rewardTick : " + rewindTick + ", current tick : " + (TickManager.Instance.GetTick() % 1024));
        // if (!(Vector3.Distance(hitState.position, hitResponseData.hitPosition) < 0.5f)) 
        // {
        //     Debugger.Log("[SERVER] [Base Character] Position Offset too far to register damage");
        //     return;
        // }
        //
        
        
        float currentHealth = DataHandler.ReduceHealth(hitResponseData.damage);
        if (currentHealth <= 0)
        {
            ulong clientID = GetComponent<NetworkObject>().OwnerClientId;
            SpawnManager spawnManager = GameManager.Instance.spawnManager;
            spawnManager.DespawnPlayer(clientID);

            //GameEvents.SendPlayerKilledEvent(GetComponent<NetworkObject>(), source);
            
            Animator.PlayDeathAnimation(true);
        }
    }

    public override void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        DataHandler.state = CharacterDataHandler.State.Dead;
        WeaponComponent.DropEquippedWeapon();
    }

    public override void Drown()
    {
        Animator.PlayDrownAnimation(true);
        SpawnManager spawnManager = GameManager.Instance.spawnManager;
        spawnManager.DespawnPlayer(NetworkObject.OwnerClientId);
    }
}
