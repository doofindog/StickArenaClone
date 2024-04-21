using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sherbert.Framework.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
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
    
    public static ConnectionManager Instance { get; private set; }
    
    public NetworkVariable<int> playersConnected = new NetworkVariable<int>();
    public int MaxPlayers => _maxConnections;
    
    private int _maxConnections = 1;
    private SessionData _sessionData;
    private PlayerData _clientData;
    [SerializeField] private SerializableDictionary<ulong, PlayerData> _playerDataCollection = new SerializableDictionary<ulong, PlayerData>();


    public async Task Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        Debugger.Log("[CONNECTION] Initialising Connection Manager");

        await UnityServices.InitializeAsync();
        
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
        
        PlayerData[] sessionDatas = _playerDataCollection.Values.ToArray();
        if (sessionDatas.Length > 0)
        {
            UpdatedPlayerCollectionsClientRPC(sessionDatas);
        }

        TeamManager.Instance.AddPlayerToTeam(clientID);

        if (_playerDataCollection.TryGetValue(clientID, out PlayerData sessionData))
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
            if (!_playerDataCollection.ContainsKey(clientID))
            {
                return;
            }
            
            _playerDataCollection.Remove(clientID);
            playersConnected.Value--;
            PlayerData[] sessionDatas = _playerDataCollection.Values.ToArray();
            UpdatedPlayerCollectionsClientRPC(sessionDatas);
        }
        
        if (IsClient && clientID == 0)
        {
            Debug.Log("[CONNECTION] Host has Disconnected");
            GameManager.Instance.TryDisconnect();
        }
    }

    [ClientRpc]
    private void UpdatedPlayerCollectionsClientRPC(PlayerData[] playerSessionDataArray)
    {
        if (NetworkManager.Singleton.IsHost || playerSessionDataArray == null || playerSessionDataArray.Length == 0) return;
        _playerDataCollection.Clear();
        foreach (PlayerData data in playerSessionDataArray)
        {
            ulong clientID = data.clientID;
            _playerDataCollection.TryAdd(clientID, data);
        }
    }

    [ClientRpc]
    private void UpdatePlayerDataClientRPC(ulong clientID, PlayerData data)
    {
        if (_playerDataCollection.ContainsKey(clientID))
        {
            _playerDataCollection[clientID] = data;
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
        PlayerData playerData = new PlayerData
        {
            clientID = approvalRequest.ClientNetworkId,
            userName = connectionPayload.userName
        };
        
        _playerDataCollection.Add(playerData.clientID, playerData);
        
        approvalResponse.Approved = true;
        
        Debugger.Log("[CONNECTION] user : " + playerData.userName + " Connection Approved");
    }

    public SerializableDictionary<ulong, PlayerData> GetPlayerSessionDataDict()
    {
        return _playerDataCollection;
    }

    public PlayerData GetPlayerData(ulong clientID)
    {
        _playerDataCollection.TryGetValue(clientID, out PlayerData sessionData);
        return sessionData;
    }

    public void ClearData()
    {
        _playerDataCollection.Clear();
        playersConnected = new NetworkVariable<int>();
    }
    
    public async Task<PlayerData> TrySignInPlayer(string playerUserName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
#if UNITY_EDITOR
            //AuthenticationService.Instance.ClearSessionToken(); 
#endif
            AuthenticationService.Instance.SwitchProfile(playerUserName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        
        Debugger.Log("[Authentication] Player ID " + AuthenticationService.Instance.PlayerId);
        return new PlayerData()
        { };
    }
    
    public void AddPlayer(ulong clientId, NetworkObject networkObject)
    {
        if (_playerDataCollection == null)
        {
            return;
        }

        PlayerData playerData = _playerDataCollection[clientId];
        playerData.networkObject = networkObject;
    }

    public NetworkObject GetPlayerNetObject(ulong clientId)
    {
        if (_playerDataCollection == null || !_playerDataCollection.ContainsKey(clientId))
        {
            return null;
        }
        
        PlayerData playerData = _playerDataCollection[clientId];
        return playerData.networkObject;
    }

    public List<PlayerData> GetAllPlayerData()
    {
        return _playerDataCollection.Values.ToList();
    }
}