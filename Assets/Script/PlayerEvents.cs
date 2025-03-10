using System;
using UnityEditor;
using UnityEngine;

public class PlayerEvents
{
    public static Action<GameObject> PlayerSpawnedEvent;
    public static void SendPlayerSpawned(GameObject pPlayer)
    {
        PlayerSpawnedEvent?.Invoke(pPlayer);
    }

    public static Action PlayerDiedEvent;
    public static void SendPlayerDied()
    {
        PlayerDiedEvent?.Invoke();
    }
    
    public static Action<PlayerData> DamageTakenEvent;
    public static void SendPlayerDamageTake(PlayerData playerData)
    {
        DamageTakenEvent?.Invoke(playerData);
    }

    public static Action WeaponFiredEvent;
    public static void SendWeaponFired()
    {
        WeaponFiredEvent?.Invoke();
    }
}
