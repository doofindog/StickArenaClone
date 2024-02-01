using System.Collections;
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
    private static readonly int NewColour = Shader.PropertyToID("newColour");


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
        PlayerSessionData playerData = connectionManager.GetPlayerSessionData(clientID);
        playerData.isJoinSession = true;
        
        SessionTeamManager.Instance.AddPlayerToTeam(playerData);
        
        Debug.Log($"[Game Session] {playerData.userName} has joined Game Session");

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
            if (sessionData.isJoinSession)
            {
                count++;
                continue;
            }
            
            return;
        }

        StartCoroutine(StartGame());
    }

    private IEnumerator StartGame()
    {
        StartGameClientRPC();
        
        yield return new WaitForSeconds(3);
        
        SpawnAllPlayers();
    }

    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
    }

    private void AddPlayer(ulong clientId, NetworkObject networkObject)
    {
        clientObjects ??= new Dictionary<ulong, NetworkObject>();
        if (clientObjects.ContainsKey(clientId))
        {
            return;
        }

        clientObjects.Add(clientId, networkObject);
    }

    private void SpawnPlayer(ulong clientID)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        GameObject playerObj = Instantiate(networkManager.NetworkConfig.PlayerPrefab);
        playerObj.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        AddPlayer(clientID, playerObj.GetComponent<NetworkObject>());
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }

    private void RespawnPlayer(ulong clientID)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
        ServerController serverController = networkObject.GetComponent<ServerController>();
        serverController.Respawn();
        
        RespawnPlayerClientRpc(clientID);
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientID)
    {
       if(IsHost) return;
       
        if (clientObjects.TryGetValue(clientID, out NetworkObject networkObject))
        {
            ClientController clientController = networkObject.GetComponent<ClientController>();
            clientController.Respawn();
        }
    }

    private void SpawnAllPlayers()
    {
        Debug.Log("[GAME SESSION] Spawning Players");
        
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