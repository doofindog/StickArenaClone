using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameSessionManager : NetworkBehaviour
{
    private static GameSessionManager _instance;
    public static GameSessionManager Singleton => _instance;

    [SerializeField] private SessionSettings _sessionSettings;
    [SerializeField] private Transform[] spawnPoints;
    
    private Dictionary<ulong, NetworkObject> clientObjects = new Dictionary<ulong, NetworkObject>();
    

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        GameEvents.PlayerSpawnedEvent += AddPlayer;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            PlayerJoinedSessionServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerJoinedSessionServerRpc(ulong clientID)
    {
        ConnectionManager connectionManager = GameNetworkManager.Instance.GetConnectionManager();
        PlayerSessionData sessionData = connectionManager.GetPlayerSessionData(clientID);
        sessionData.IsJoinSession = true;

        if (IsServer)
        {
            StartGameSession();
        }
    }

    private void StartGameSession()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        ConnectionManager connectionManager = GameNetworkManager.Instance.GetConnectionManager();
        int count = 0;
        foreach (ulong clientID in networkManager.ConnectedClients.Keys)
        {
            PlayerSessionData sessionData = connectionManager.GetPlayerSessionData(clientID);
            if (sessionData.IsJoinSession)
            {
                count++;
                continue;
            }

            Debug.Log("Waiting for players : " + count + " / " + connectionManager.PlayersConnected);
            return;
        }
        
        SpawnAllPlayers();
    }

    private void AddPlayer(ulong clientId, NetworkObject networkObject)
    {
        if (clientObjects == null)
        {
            clientObjects = new Dictionary<ulong, NetworkObject>();
        }
        
        if(clientObjects.ContainsKey(clientId)) return;

        clientObjects.Add(clientId, networkObject);
    }

    private void SpawnPlayer(ulong clientID)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        GameObject playerPrefab = Instantiate(networkManager.NetworkConfig.PlayerPrefab);
        playerPrefab.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        AddPlayer(clientID, playerPrefab.GetComponent<NetworkObject>());
        playerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }

    private void RespawnPlayer(ulong clientID)
    {
        Debug.Log("called");
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
        ServerController serverController = networkObject.GetComponent<ServerController>();
        serverController.Respawn();
        
        RespawnPlayerClientRpc(clientID);
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientID)
    {
        Debug.Log("called RPC");
        if (clientObjects.TryGetValue(clientID, out NetworkObject networkObject))
        {
            ClientController clientController = networkObject.GetComponent<ClientController>();
            clientController.Respawn();
        }
    }

    private void SpawnAllPlayers()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        foreach (ulong clientID in networkManager.ConnectedClients.Keys)
        {
            SpawnPlayer(clientID);
        }
    }

    public void DespawnPlayer(ulong clientId)
    {
        if(!IsServer) return;
        
        if (clientObjects.TryGetValue(clientId, out NetworkObject obj))
        {
            if(obj.TryGetComponent(out ServerController controller))
            {
                controller.Die();
            }
        }
        
        SendDespawnClientRpc(clientId);
    }

    [ClientRpc]
    private void SendDespawnClientRpc(ulong clientID)
    {
        if (clientObjects.TryGetValue(clientID, out NetworkObject networkObject))
        {
            ClientController controller = networkObject.GetComponent<ClientController>();
            if (controller != null)
            {
                controller.Die();
            }
        }
    }

    public SessionSettings GetSessionSettings()
    {
        return _sessionSettings;
    }

    [ServerRpc(RequireOwnership =false)]
    public void RequestSpawnPlayerServerRPC(ulong clientID)
    {
        RespawnPlayer(clientID);
    }
}