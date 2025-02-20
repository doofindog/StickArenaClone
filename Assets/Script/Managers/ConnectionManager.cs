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
    
    public static ConnectionManager Instance { get; private set; }

    public void Init()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        Debugger.Log("[CONNECTION] Initialising Connection Manager");
        
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += HandleOnServerStarted;
    }
    
    private void HandleOnServerStarted()
    {
        if (!IsServer) return;
        Debug.Log("[Connection] Server Started");
    }

    private void HandleClientConnected(ulong pClientID)
    {
        Debugger.Log("[CONNECTION] Client has Connected");
    }

    private void HandleClientDisconnected(ulong pClientID)
    {
        Debugger.Log("[CONNECTION] Client has Disconnected");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest pApprovalRequest, NetworkManager.ConnectionApprovalResponse pApprovalResponse)
    {

        if (!IsServer)
        {
            return;
        }

        pApprovalResponse.Approved = true;

        if (pApprovalResponse.Approved)
        {
            string payloadJSON = System.Text.Encoding.UTF8.GetString(pApprovalRequest.Payload);
            ConnectionPayload connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payloadJSON);

            PublicPlayerData publicPlayerData = new PublicPlayerData()
            {
                username = connectionPayload.userName,
            };

            ClientData clientData = new ClientData()
            {
                clientID = pApprovalRequest.ClientNetworkId,
                publicPlayerData = publicPlayerData,
            };

            SessionManager.Instance.AddClientToSession(clientData);
        }
    }

    public void TryJoin(string pUsername, string joinCode = "")
    {
        ConnectionPayload payload = new ConnectionPayload() { userName = pUsername };
        string payloadJSON = JsonUtility.ToJson(payload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.UTF8.GetBytes(payloadJSON);
        NetworkManager.Singleton.StartClient();

        CustomNetworkEvents.SendNetworkStartedEvent();
    }

    public void StartServer(string username="")
    {
        NetworkManager.Singleton.StartServer();
    }

    public void TryDisconnect()
    {
        NetworkManager.Singleton.Shutdown();
        CustomNetworkEvents.SendDisconnectedEvent();
    }
}