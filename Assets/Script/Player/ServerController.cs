using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class ServerController : NetController, ITickableEntity, IDamageableEntity
{
    private NetInputProcessor _netInputProcessor;
    private NetStateProcessor _netStateProcessor;
    private Queue<HitResponseData> _damageProcessor;

    public override void Awake()
    {
        base.Awake();

        _netInputProcessor = GetComponent<NetInputProcessor>();
        _netStateProcessor = GetComponent<NetStateProcessor>();
        _damageProcessor = new Queue<HitResponseData>();
        
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
        
        while (_damageProcessor.Count > 0)
        {
            HitResponseData hitResponseData = _damageProcessor.Dequeue();
            ulong ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(hitResponseData.sourceID);
            float rewindTime = Time.realtimeSinceStartup * 1000 - ping;
            int rewindTick = Convert.ToInt32((rewindTime * (TickManager.Instance.GetTick() % 1024)) / (Time.realtimeSinceStartup * 1000));
        
            NetStatePayLoad rewindState = _netStateProcessor.GetStateAtTick(rewindTick);
        
            Debug.Log($"Hit Time {hitResponseData.hitTime}, Server Time : {NetworkManager.ServerTime.Time}");
            if (!(Vector3.Distance(rewindState.position, hitResponseData.hitPosition) < 0.8f)) 
            {
                Debugger.Log($"[SERVER] [Base Character] {rewindState.position} , {hitResponseData.hitPosition} Position Offset too far to register damage");
                return;
            }
        
            float currentHealth = DataHandler.ReduceHealth(hitResponseData.damage);
            if (currentHealth <= 0)
            {
                ulong clientID = GetComponent<NetworkObject>().OwnerClientId;
                SpawnManager spawnManager = GameManager.Instance.spawnManager;
                spawnManager.DespawnPlayer(clientID);

                //GameEvents.SendPlayerKilledEvent(GetComponent<NetworkObject>(), source);
            
                Animator.PlayDeathAnimation(true);
                _damageProcessor.Clear();
            }
        }
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

        if (!(GetComponent<CharacterDataHandler>().health.Value <= 0))
        {
            _damageProcessor.Enqueue(hitResponseData);
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

    public void AddHealth()
    {
        DataHandler.health.Value++;
    }
}
