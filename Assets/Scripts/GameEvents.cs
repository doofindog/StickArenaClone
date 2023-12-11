using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static Action<GameObject> PlayerConnectedEvent;

    public static void SendPlayerConnected(GameObject playerObj)
    {
        PlayerConnectedEvent?.Invoke(playerObj);
    }

    public static Action WeaponFiredEvent;
    public static void SendWeaponFired()
    {
        WeaponFiredEvent?.Invoke();
        
    }
}
