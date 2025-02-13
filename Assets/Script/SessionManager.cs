using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Unity.Netcode;

public class SessionManager : NetworkBehaviour
{
    public NetworkVariable<int> countDownTimer;
    public List<ClientData> clientDataCollection = new List<ClientData>();
    public NetworkList<PublicPlayerData> publicPlayerDataCollection;

    public static SessionManager Instance { get; private set; }



    private void HandleClientDisconnected(ulong pClientId)
    {
        ClientData clientData = GetClientData(pClientId);
        clientData.isConnected = false;
    }

    private void HandleClientConnected(ulong pClientId)
    {
#if SERVER
        ClientData clientData = GetClientData(pClientId);
        clientData.isConnected = true;

        TryStartSession();
#endif
    }

    private void TryStartSession()
    {
        CustomNetworkEvents.SendAllPlayersConnectedEvent();
        StartSessionClientRPC();
    }

    [ClientRpc]
    private void StartSessionClientRPC()
    {
        CustomNetworkEvents.SendAllPlayersConnectedEvent();
    }

    private void CleanData()
    {
        clientDataCollection.Clear();
    }

    private void Awake()
    {
        SessionManager.Instance = this;

        countDownTimer = new NetworkVariable<int>();
        clientDataCollection = new List<ClientData>();
        publicPlayerDataCollection = new NetworkList<PublicPlayerData>(new List<PublicPlayerData>());
    }

    public void Init()
    {
        NetworkManager.OnServerStarted += HandleServerStarted;
        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        NetworkManager.OnServerStarted -= HandleServerStarted;
    }

    public void CreateNewSession()
    {
        CleanData();

        countDownTimer.Value = GameManager.Instance.GetSessionSettings().countDownTime;
    }

    public void EndSession()
    {
        CleanData();
    }

    public void HandleServerStarted()
    {
        SessionManager.Instance.CreateNewSession();
    }

    public ClientData GetClientData(ulong pClientID)
    {
        if(clientDataCollection == null)
        {
            return ClientData.NullableData;
        }
        
        foreach(ClientData clientData in clientDataCollection)
        {
            if (clientData.clientID == pClientID)
            {
                return clientData;
            }
        }

        return ClientData.NullableData;
    }

    public int GetConnectedClientCount()
    {
        if (clientDataCollection == null) return 0;

        return clientDataCollection.Count;
    }

    public void AddClientToSession(ClientData pClientData)
    {
        if (clientDataCollection == null)
        {
            clientDataCollection = new List<ClientData>();
        }

        for(int i = 0; i < clientDataCollection.Count; i++)
        {
            ClientData clientData = clientDataCollection[i];
            if(clientData.Equals(pClientData))
            {
                return;
            }
        }

        clientDataCollection.Add(pClientData);
        publicPlayerDataCollection.Add(pClientData.publicPlayerData);
    }

    public void RemoveClientFromSession(ClientData pClientData)
    {
        if (clientDataCollection == null)
        {
            return;
        }

        for(int i = 0; i< clientDataCollection.Count; i++)
        {
            if (!clientDataCollection[i].Equals(pClientData))
            {
                clientDataCollection.RemoveAt(i);
            }
        }
    }
}
