using System.Collections;
using System.Collections.Generic;using System.Data;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConnectionManager : Singleton<ConnectionManager>
{
    public enum ConnectionState
    {
        Online,
        Connecting,
        Connected,
        ConnectionFailed,
    }

    
    [SerializeField] private int maxConnections = 2;
    
    private HashSet<ulong> _playersConnectedID = new HashSet<ulong>();
    private Dictionary<ulong, PlayerSessionData> _playerSessionDataCollection = new Dictionary<ulong, PlayerSessionData>();
    
    public int PlayersConnected { get; set; }
    
    public void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
    }

    private void HandleServerStarted()
    {
        Debug.Log("Server Started");
    }

    private void HandleClientConnected(ulong clientID)
    {
        Debug.Log("Client Connected : " + clientID);
        
        if (NetworkManager.Singleton.IsServer)
        {
            if (_playerSessionDataCollection.TryGetValue(clientID, out PlayerSessionData sessionData))
            {
                sessionData.IsConnected = true;
                PlayersConnected++;
                if (PlayersConnected == maxConnections)
                {
                    NetworkSceneManager sceneManager = NetworkManager.Singleton.SceneManager;
                    sceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
                } 
            }
        }
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        string disconnectReason = NetworkManager.Singleton.DisconnectReason;
        if (string.IsNullOrEmpty(disconnectReason))
        {
            Debug.Log("Could not Connect to Server");
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        if (PlayersConnected >= maxConnections)
        {
            return;
        }
        
        PlayerSessionData playerSessionData = new PlayerSessionData
        {
            ClientID = approvalRequest.ClientNetworkId
        };
        
        _playerSessionDataCollection.Add(playerSessionData.ClientID, playerSessionData);
        approvalResponse.Approved = true;
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
    
    public static void TryJoin()
    {
        NetworkManager.Singleton.StartClient();
    }

    public static void TryStartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    public static void TryStartServer()
    {
        NetworkManager.Singleton.StartServer();
    }
}
