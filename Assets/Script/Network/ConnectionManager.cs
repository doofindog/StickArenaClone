using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ConnectionManager : NetworkBehaviour
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionFailed,
    }
    
    public NetworkVariable<int> playersConnected = new NetworkVariable<int>();
    public int MaxPlayers => _maxConnections;
    
    
    private int _maxConnections = 1;
    private Dictionary<ulong, PlayerSessionData> _playerSessionDataCollection = new Dictionary<ulong, PlayerSessionData>();


    public void Init()
    {
        Debugger.Log("[CONNECTION] Initialising Connection Manager");
        
        DontDestroyOnLoad(this.gameObject);
        
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += HandleOnServerStarted;

        _maxConnections = GameManager.Instance.GetSessionSettings().maxConnections;
    }
    
    
    private void HandleOnServerStarted()
    {
        playersConnected.Value = 0;
    }

    private void HandleClientConnected(ulong clientID)
    {
        if (NetworkManager.Singleton.IsServer)
        { 
            HandleClientConnectedOnServer(clientID);
        }
    }

    private void HandleClientConnectedOnServer(ulong clientID)
    {
        Debugger.Log("[CONNECTION] Client has Connected");
        
        PlayerSessionData[] sessionDatas = _playerSessionDataCollection.Values.ToArray();
        if (sessionDatas.Length > 0)
        {
            UpdatedPlayerCollectionsClientRPC(sessionDatas);
        }

        TeamManager.Instance.AddPlayerToTeam(clientID);

        if (_playerSessionDataCollection.TryGetValue(clientID, out PlayerSessionData sessionData))
        {
            sessionData.isConnected = true;
            playersConnected.Value ++;
            if (playersConnected.Value == _maxConnections)
            {
                StartCoroutine(SendClient());
            } 
        }
    }

    private IEnumerator SendClient()
    {
        yield return new WaitForSeconds(3);
        
        SendAllClientsConnectedClientRPC();
    }

    [ClientRpc]
    private void SendAllClientsConnectedClientRPC()
    {
        CustomNetworkEvents.SendAllPlayersConnectedEvent();
    }
    
    private void HandleClientDisconnected(ulong clientID)
    {
        if (IsServer)
        {
            if (!_playerSessionDataCollection.ContainsKey(clientID))
            {
                return;
            }
            
            _playerSessionDataCollection.Remove(clientID);
            playersConnected.Value--;
            PlayerSessionData[] sessionDatas = _playerSessionDataCollection.Values.ToArray();
            UpdatedPlayerCollectionsClientRPC(sessionDatas);
        }
        
        if (IsClient && clientID == 0)
        {
            Debug.Log("[CONNECTION] Host has Disconnected");
            GameManager.Instance.TryDisconnect();
        }
    }

    [ClientRpc]
    private void UpdatedPlayerCollectionsClientRPC(PlayerSessionData[] playerSessionDataArray)
    {
        if (NetworkManager.Singleton.IsHost || playerSessionDataArray == null || playerSessionDataArray.Length == 0) return;
        _playerSessionDataCollection.Clear();
        foreach (PlayerSessionData data in playerSessionDataArray)
        {
            ulong clientID = data.clientID;
            _playerSessionDataCollection.TryAdd(clientID, data);
        }
    }

    [ClientRpc]
    private void UpdatePlayerDataClientRPC(ulong clientID, PlayerSessionData sessionData)
    {
        if (_playerSessionDataCollection.ContainsKey(clientID))
        {
            _playerSessionDataCollection[clientID] = sessionData;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        if (playersConnected.Value >= _maxConnections)
        {
            Debugger.Log("[CONNECTION] Max Connections Reached");
            return;
        }
        
        string payloadJson = System.Text.Encoding.ASCII.GetString(approvalRequest.Payload);
        ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payloadJson);
        PlayerSessionData playerSessionData = new PlayerSessionData
        {
            clientID = approvalRequest.ClientNetworkId,
            userName = connectionPayload.userName
        };
        
        _playerSessionDataCollection.Add(playerSessionData.clientID, playerSessionData);
        
        approvalResponse.Approved = true;
        
        Debugger.Log("[CONNECTION] user : " + playerSessionData.userName + " Connection Approved");
    }

    public Dictionary<ulong, PlayerSessionData> GetPlayerSessionDataDict()
    {
        return _playerSessionDataCollection;
    }

    public PlayerSessionData GetPlayerSessionData(ulong clientID)
    {
        _playerSessionDataCollection.TryGetValue(clientID, out PlayerSessionData sessionData);
        return sessionData;
    }

    public void ClearData()
    {
        _playerSessionDataCollection.Clear();
        playersConnected = new NetworkVariable<int>();
    }
}