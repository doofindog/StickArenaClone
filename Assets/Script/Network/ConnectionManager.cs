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
    [SerializeField] private int playersConnected = 0;
    
    private Dictionary<ulong, PlayerSessionData> PlayerSessionDataCollection = new Dictionary<ulong, PlayerSessionData>();
    

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
        Debug.Log("Client Connected");
        if (NetworkManager.Singleton.IsServer)
        {
            playersConnected++;
            if (playersConnected == maxConnections)
            {
                //Start Game;
                Debug.Log("Starting Game");
                NetworkSceneManager sceneManager = NetworkManager.Singleton.SceneManager;
                sceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
            }
        }
    }

    private void HandleClientDisconnected(ulong clientID)
    {
        string disconnectReason = NetworkManager.Singleton.DisconnectReason;
        if (string.IsNullOrEmpty(disconnectReason))
        {
            //m_ConnectStatusPublisher.Publish(ConnectStatus.StartClientFailed);
            Debug.Log("Could not Connect to Server");
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest approvalRequest, NetworkManager.ConnectionApprovalResponse approvalResponse)
    {
        bool canConnect = playersConnected > maxConnections &&
                             approvalRequest.ClientNetworkId != NetworkManager.Singleton.LocalClientId;
        
        if (canConnect)
        {
            approvalResponse.Approved = false;
            approvalResponse.Reason = "Max Players Reached";
            return;
        }
        
        PlayerSessionData playerSessionData = new PlayerSessionData
        {
            ClientID = approvalRequest.ClientNetworkId
        };

        PlayerSessionDataCollection.Add(playerSessionData.ClientID, playerSessionData);
        approvalResponse.Approved = true;
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
