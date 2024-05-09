using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ServerController : NetController, ITickableEntity, IDamageableEntity
{
    private NetInputProcessor _netInputProcessor;
    private NetStateProcessor _netStateProcessor;
    private Queue<HitResponseData> _damageProcessor;
    
    
    private Vector3 gizmoPosition;

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
    }

    private void HandleInputProcessed(NetInputPayLoad inputPayLoad)
    {
        if (!IsOwner)
        {
            ProcessMovement(inputPayLoad);
        }

        NetStatePayLoad statePayLoad = new NetStatePayLoad()
        {
            time = NetworkManager.ServerTime.TimeAsFloat,
            tick = inputPayLoad.tick,
            position = interpolate ? newPosition : transform.position,
            aimAngle = inputPayLoad.aimAngle,
            dodge = inputPayLoad.dodgePressed,
            isDodge = DataHandler.isDodge,
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
        bool failedCheck = GetComponent<CharacterDataHandler>().health.Value <= 0 ||
                           hitResponseData == null ||
                           _netStateProcessor.frameHistory.First == null ||
                           _netStateProcessor.frameHistory.Last == null;

        if (failedCheck)
        {
            Debug.Log("[SSR] Failed Check");
            return;
        }

        //Frame history of the hit character
        LinkedList<NetStatePayLoad> history = _netStateProcessor.frameHistory;
        float oldestHistoryTime = history.Last.Value.time;
        float newestHistoryTime = history.Last.Value.time;
        if (oldestHistoryTime > hitResponseData.hitTime)
        {
            //Too Far Back
            Debugger.Log($"[SSR] History too Far Back Skipping Rewind : {oldestHistoryTime} , {hitResponseData.hitTime}");
            return;
        }

        bool scheduleRewind = true;
        NetStatePayLoad frameToCheck = new NetStatePayLoad();
        if (newestHistoryTime <= hitResponseData.hitTime)
        {
            frameToCheck = history.First.Value;
            scheduleRewind = false;
        }

        LinkedListNode<NetStatePayLoad> younger = history.First;
        LinkedListNode<NetStatePayLoad> older = history.Last;

        while (older.Value.time > hitResponseData.hitTime)
        {
            if(older.Previous == null) break;
            older = older.Previous;

            if (older.Value.time > hitResponseData.hitTime)
            {
                younger = older;
            }
        }
            
        //Confirm Hit After Getting Frame to check and Interpolation
        if (scheduleRewind)
        {
            float distance = younger.Value.time - older.Value.time;
            float interpFraction = Mathf.Clamp01((hitResponseData.hitTime - older.Value.time) / distance);
            frameToCheck.position = Vector3.Lerp(older.Value.position, younger.Value.position, interpFraction);
            frameToCheck.time = hitResponseData.hitTime;
        }
        
        //PredictPath
        if (ConfirmHit(frameToCheck, hitResponseData)) 
        {
            Debugger.Log("[SSR] Hit Confirmed");
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
        else
        {
            Debugger.Log("[SSR] Hit Failed");
        }


        //StartCoroutine(ConfirmHitCoroutine(frameToCheck, hitResponseData));
    }


    private bool ConfirmHit(NetStatePayLoad frameToCheck, HitResponseData hitData)
    {
        Vector3 simPosition = hitData.traceStart;
        Quaternion simRotation = hitData.projectileRotation;
        float simTimer = 0.0f;
        float maxSimTime = 5.0f;
        float frequency = 15.0f;
        float subStep = maxSimTime / Mathf.CeilToInt(frequency * maxSimTime);
        while (simTimer <= maxSimTime)
        {
            simTimer += subStep;
            simPosition += hitData.projectileDirection.normalized * hitData.hitVelocity;
            gizmoPosition = simPosition;
            Collider2D col = Physics2D.OverlapBox(simPosition, Vector2.one, Quaternion.Angle(Quaternion.identity, simRotation));
            if (col != null)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator ConfirmHitCoroutine(NetStatePayLoad frameToCheck, HitResponseData hitData)
    {
        Vector3 simPosition = hitData.traceStart;
        float simTimer = 0.0f;
        float maxSimTime = 5.0f;
        float frequency = 15.0f;
        float subStep = maxSimTime / Mathf.CeilToInt(frequency * maxSimTime);
        while (simTimer <= maxSimTime)
        {
            simTimer += subStep;
            simPosition += hitData.projectileDirection.normalized * hitData.hitVelocity;
            gizmoPosition = simPosition;
            yield return new WaitForSeconds(0);
        }
    }

    public override void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }

    public override void OnRespawn()
    {
        base.OnRespawn();
        
        GameObject weaponPrefab = GameManager.Instance.GetSessionSettings().defaultWeapon;
        GameObject weaponObj = SpawnManager.Instance.SpawnObject(weaponPrefab, SpawnManager.SpawnType.NETWORK, Vector3.zero,
            Quaternion.identity);
        weaponObj.GetComponent<NetworkObject>().Spawn();
        Weapon weapon = weaponObj.GetComponent<Weapon>();
        GetComponent<WeaponComponent>().EquipWeapon(weapon);
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
    
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(gizmoPosition, .25f);
    }
}
