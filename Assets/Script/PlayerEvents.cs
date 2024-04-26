using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents
{
    public static Action<GameObject> PlayerSpawnedEvent;
    public static void SendPlayerSpawned(GameObject playerObj)
    {
        PlayerSpawnedEvent?.Invoke(playerObj);
    }

    public static Action PlayerDiedEvent;
    public static void SendPlayerDied()
    {
        PlayerDiedEvent?.Invoke();
    }
    
    public static Action<CharacterDataHandler> DamageTakenEvent;
    public static void SendPlayerDamageTake(CharacterDataHandler playerData)
    {
        DamageTakenEvent?.Invoke(playerData);
    }
}
