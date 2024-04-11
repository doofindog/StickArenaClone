using System;
using Unity.Netcode;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static Action SplashCompleted;

    public static void SendSplashCompleted()
    {
        SplashCompleted?.Invoke();
    }
    
    public static Action PreparingArenaEvent;
    public static void SendPreparingArenaEvent()
    {
        PreparingArenaEvent?.Invoke();
    }
    
    public static Action OnGameStartEvent;
    public static void SendStartGameEvent()
    {
        OnGameStartEvent?.Invoke();
    }
    
    public static Action<NetworkObject,NetworkObject> PlayerDiedEvent;
    public static void SendPlayerKilledEvent(NetworkObject player, NetworkObject source)
    {
        PlayerDiedEvent?.Invoke(player, source);
    }
    
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

    public static Action<ulong> CrownAcquiredEvent;
    public static void SendCrownAcquired(ulong clientId)
    {
        CrownAcquiredEvent?.Invoke(clientId);
    }

    public static Action<TeamType> TeamWonEvent;
    public static void SendTeamWonEvent(TeamType teamType)
    {
        TeamWonEvent?.Invoke(teamType);
    }

    public static Action OnGameOverEvent;
    public static void SendGameOver()
    {
        OnGameOverEvent?.Invoke();
    }
}
