using System;
using Unity.Netcode;
using UnityEngine;

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

    public static Action StartGameEvent;

    public static void SendStartGameEvent()
    {
        StartGameEvent?.Invoke();
    }
}
