using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEvents
{
    public static Action<CharacterDataHandler, float> DamageTakenEvent;
    public static void SendPlayerDamageTake(CharacterDataHandler playerData, float damageTaken)
    {
        DamageTakenEvent?.Invoke(playerData, damageTaken);
    }
}
