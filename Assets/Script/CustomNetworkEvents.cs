using System;
using UnityEngine;

public class CustomNetworkEvents : MonoBehaviour
{

    public static Action DisconnectedEvent;
    public static void SendDisconnectedEvent()
    {
        DisconnectedEvent?.Invoke();
    }

    public static Action NetworkStartedEvent;

    public static void SendNetworkStartedEvent()
    {
        Debugger.Log("[Events] Called");
        NetworkStartedEvent?.Invoke();
    }
}
