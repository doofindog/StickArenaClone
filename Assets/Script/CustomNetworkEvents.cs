using System;
using Unity.Services.Authentication;
using UnityEngine;

public class CustomNetworkEvents : MonoBehaviour
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

    public static Action NetworkStartedEvent;

    public static void SendNetworkStartedEvent()
    {
        Debugger.Log("[Events] Called");
        NetworkStartedEvent?.Invoke();
    }

    public static Action<ClientData> ClientConnectedEvent;
    public static void SendClientConnectedEvent(ClientData pClientData)
    {
        ClientConnectedEvent?.Invoke(pClientData);
    }

    public static Action<ClientData> ClientDisconnectedEvent;
    public static void SendClientDisconnectedEvent(ClientData pClientData)
    {
        ClientDisconnectedEvent?.Invoke(pClientData);
    }

    public static Action<ClientData[]> clientCollectionUpdatedEvent;
    public static void SendClientCollectionUpdated(ClientData[] clientCollection)
    {
        clientCollectionUpdatedEvent?.Invoke(clientCollection);
    }
}
