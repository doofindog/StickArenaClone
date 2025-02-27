using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ServerController : NetController, ITickableEntity
{
    public override void Awake()
    {


        base.Awake();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Destroy(this);
            return;
        }

        playerData.Init();
        playerFeature.Init(this);
        weaponComponent.Init(playerComponent.arm, playerComponent.weaponHolder);

        weaponComponent.GiveDefaultWeapon();

        TickManager.Instance.AddEntity(this);
    }

    public override void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }

    public void UpdateTick(int tick)
    {
        NetInputPayLoad[] processedInputPayload = inputProcessor.ProcessInputs();
        NetStatePayLoad[] processedStatePayload = new NetStatePayLoad[processedInputPayload.Length];
        for (int i = 0; i < processedInputPayload.Length; i++)
        {
            NetInputPayLoad inputPayload = processedInputPayload[i];
            playerFeature.ProcessFeature(inputPayload);

            NetStatePayLoad previousPayload = stateProcessor.LastProcessedState;
            NetStatePayLoad statePayLoad = new NetStatePayLoad()
            {
                inputSequence = inputPayload.payloadSequence,
                time = NetworkManager.ServerTime.TimeAsFloat,
                tick = inputPayload.tick,
                position = transform.position,
                positionDelta = transform.position - previousPayload.position,
                aimAngle = inputPayload.aimAngle,
                shootPressed = inputPayload.shootPressed,
            };

            processedStatePayload[i] = stateProcessor.AddState(statePayLoad);
        }

        stateProcessor.SendProcessedStateClientRPC(processedStatePayload);
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

        playerData.state = PlayerData.State.Dead;
    }
}

    //public override void TakeDamage(HitResponseData hitResponseData)
    //{
    //    // Server Side Rewind
    //    bool failedCheck = GetComponent<PlayerData>().health.Value <= 0 ||
    //                       hitResponseData == null ||
    //                       m_netStateProcessor.frameHistory.First == null ||
    //                       m_netStateProcessor.frameHistory.Last == null;

    //    if (failedCheck)
    //    {
    //        Debug.Log("[SSR] Failed Check");
    //        return;
    //    }

    //    //Frame history of the hit character
    //    LinkedList<NetStatePayLoad> history = m_netStateProcessor.frameHistory;
    //    float oldestHistoryTime = history.Last.Value.time;
    //    float newestHistoryTime = history.Last.Value.time;
    //    if (oldestHistoryTime > hitResponseData.hitTime)
    //    {
    //        //Too Far Back
    //        Debugger.Log($"[SSR] History too Far Back Skipping Rewind : {oldestHistoryTime} , {hitResponseData.hitTime}");
    //        return;
    //    }

    //    bool scheduleRewind = true;
    //    NetStatePayLoad frameToCheck = new NetStatePayLoad();
    //    if (newestHistoryTime <= hitResponseData.hitTime)
    //    {
    //        frameToCheck = history.First.Value;
    //        scheduleRewind = false;
    //    }

    //    LinkedListNode<NetStatePayLoad> younger = history.First;
    //    LinkedListNode<NetStatePayLoad> older = history.Last;

    //    while (older.Value.time > hitResponseData.hitTime)
    //    {
    //        if(older.Previous == null) break;
    //        older = older.Previous;

    //        if (older.Value.time > hitResponseData.hitTime)
    //        {
    //            younger = older;
    //        }
    //    }

    //    //Confirm Hit After Getting Frame to check and Interpolation
    //    if (scheduleRewind)
    //    {
    //        float distance = younger.Value.time - older.Value.time;
    //        float interpFraction = Mathf.Clamp01((hitResponseData.hitTime - older.Value.time) / distance);
    //        frameToCheck.position = Vector3.Lerp(older.Value.position, younger.Value.position, interpFraction);
    //        frameToCheck.time = hitResponseData.hitTime;
    //    }

    //    //PredictPath
    //    if (ConfirmHit(frameToCheck, hitResponseData)) 
    //    {
    //        Debugger.Log("[SSR] Hit Confirmed");
    //        float currentHealth = DataHandler.ReduceHealth(hitResponseData.damage);
    //        if (currentHealth <= 0)
    //        {
    //            ulong clientID = GetComponent<NetworkObject>().OwnerClientId;
    //            SpawnManager spawnManager = GameManager.Instance.spawnManager;
    //            //spawnManager.DespawnPlayer(clientID);

    //            //GameEvents.SendPlayerKilledEvent(GetComponent<NetworkObject>(), source);

    //            Animator.PlayDeathAnimation(true);
    //            m_damageProcessor.Clear();
    //        }
    //    }
    //    else
    //    {
    //        Debugger.Log("[SSR] Hit Failed");
    //    }
    //}


    //private bool ConfirmHit(NetStatePayLoad frameToCheck, HitResponseData hitData)
    //{
    //    Vector3 simPosition = hitData.traceStart;
    //    Quaternion simRotation = hitData.projectileRotation;
    //    float simTimer = 0.0f;
    //    float maxSimTime = 5.0f;
    //    float frequency = 15.0f;
    //    float subStep = maxSimTime / Mathf.CeilToInt(frequency * maxSimTime);
    //    while (simTimer <= maxSimTime)
    //    {
    //        simTimer += subStep;
    //        simPosition += hitData.projectileDirection.normalized * hitData.hitVelocity;
    //        gizmoPosition = simPosition;
    //        Collider2D col = Physics2D.OverlapBox(simPosition, Vector2.one, Quaternion.Angle(Quaternion.identity, simRotation));
    //        if (col != null)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}

    //private IEnumerator ConfirmHitCoroutine(NetStatePayLoad frameToCheck, HitResponseData hitData)
    //{
    //    Vector3 simPosition = hitData.traceStart;
    //    float simTimer = 0.0f;
    //    float maxSimTime = 5.0f;
    //    float frequency = 15.0f;
    //    float subStep = maxSimTime / Mathf.CeilToInt(frequency * maxSimTime);
    //    while (simTimer <= maxSimTime)
    //    {
    //        simTimer += subStep;
    //        simPosition += hitData.projectileDirection.normalized * hitData.hitVelocity;
    //        gizmoPosition = simPosition;
    //        yield return new WaitForSeconds(0);
    //    }
    //}
