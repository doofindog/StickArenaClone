using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class GameEvents : MonoBehaviour
{
    public static Action<ulong, NetworkObject> PlayerSpawnedEvent;

    public static void SendPlayerSpawned(ulong clientId, NetworkObject networkObject)
    {
        PlayerSpawnedEvent?.Invoke(clientId, networkObject);
    }
    
    public static Action WeaponFiredEvent;
    public static void SendWeaponFired()
    {
        WeaponFiredEvent?.Invoke();
    }

    public static Action GameSessionStartedEvent;

    public static void SendGameSessionStarted()
    {
        GameSessionStartedEvent?.Invoke();
    }
}
