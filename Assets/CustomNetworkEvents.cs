using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public static class CustomNetworkEvents
{
    public static Action AllPlayersConnectedEvent;

    public static void SendAllPlayersConnectedEvent()
    {
        AllPlayersConnectedEvent?.Invoke();
    }

    public static Action DisconnectedEvent;

    public static void SendDisconnectedEvent()
    {
        DisconnectedEvent?.Invoke();
    }
}
