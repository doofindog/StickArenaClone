using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class ConnectionManager : NetworkBehaviour
{
    public static ConnectionManager Instance;
    
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        ConnectionFailed,
    }
    
    [SerializeField] private int maxConnections = 1;
    
    private Dictionary<ulong, PlayerSessionData> _playerSessionDataCollection = new Dictionary<ulong, PlayerSessionData>();
    
    public NetworkVariable<int> playersConnected = new NetworkVariable<int>();
    public int MaxPlayers => maxConnections;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(this.gameObject);
    }

    public void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += HandleOnServerStarted;
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
            if (playersConnected.Value == maxConnections)
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
        
        if (IsClient)
        {
            if (clientID == 0)
            {
                Debug.Log("[CONNECTION] Host has Disconnected");
                TryDisconnect();
            }
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
        if (playersConnected.Value >= maxConnections) return;
        
        string payloadJson = System.Text.Encoding.ASCII.GetString(approvalRequest.Payload);
        ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payloadJson);
        PlayerSessionData playerSessionData = new PlayerSessionData
        {
            clientID = approvalRequest.ClientNetworkId,
            userName = connectionPayload.userName
        };
        
        _playerSessionDataCollection.Add(playerSessionData.clientID, playerSessionData);
        
        approvalResponse.Approved = true;
        
        Debug.Log("[CONNECTION] user : " + playerSessionData.userName + " Connection Approved");
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

    private void ClearData()
    {
        _playerSessionDataCollection.Clear();
    }
    
    public static void TryJoin(string username)
    {
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.StartClient();
        
        CustomNetworkEvents.SendNetworkStartedEvent();
    }

    public static void TryStartHost(string username)
    {
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.StartHost();
        
        CustomNetworkEvents.SendNetworkStartedEvent();
    }
    
    public static void TryDisconnect()
    {
        Instance.ClearData();
        NetworkManager.Singleton.Shutdown();
        CustomNetworkEvents.SendDisconnectedEvent();
    }
}