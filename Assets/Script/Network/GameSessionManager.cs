using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameSessionManager : NetworkBehaviour
{
    private static GameSessionManager _instance;
    public static GameSessionManager Singleton => _instance;

    [SerializeField] private SessionSettings _sessionSettings;
    [SerializeField] private List<SpawnData> spawnLocations;
    
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
        PlayerSessionData playerData = connectionManager.GetPlayerSessionData(clientID);
        playerData.isJoinSession = true;
        
        TeamManager.Instance.AddPlayerToTeam(playerData);
        
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

    public void AddPlayer(ulong clientId, NetworkObject networkObject)
    {
        clientObjects ??= new Dictionary<ulong, NetworkObject>();
        if (clientObjects.ContainsKey(clientId))
        {
            return;
        }

        clientObjects.Add(clientId, networkObject);
    }

    public NetworkObject GetPlayerNetObject(ulong clientID)
    {
        clientObjects.TryGetValue(clientID, out NetworkObject netObj);
        return netObj;
    }

    private Transform GetSpawnLocation(TeamType type)
    {
        return (from location in spawnLocations where type == location.teamType select location.spawnLocation[Random.Range(0, location.spawnLocation.Length)]).FirstOrDefault();
    }

    private void SpawnPlayer(ulong clientID)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        GameObject playerObj = Instantiate(networkManager.NetworkConfig.PlayerPrefab);
        TeamType playerTeam = TeamManager.Instance.GetTeamFromID(clientID).teamType;
        Transform spawnTransform = GetSpawnLocation(playerTeam);
        playerObj.transform.position = spawnTransform.position;
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }

    private void RespawnPlayer(ulong clientID)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
        ServerController serverController = networkObject.GetComponent<ServerController>();
        TeamType playerTeam = TeamManager.Instance.GetTeamFromID(clientID).teamType;
        Transform spawnTransform = GetSpawnLocation(playerTeam);
        serverController.transform.position = spawnTransform.position;
        serverController.OnRespawn();
        
        RespawnPlayerClientRpc(clientID);
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientID)
    {
        if (!clientObjects.TryGetValue(clientID, out NetworkObject networkObject)) return;
        
        
        ClientController clientController = networkObject.GetComponent<ClientController>();
        if (clientController != null)
        {
            clientController.OnRespawn();
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
                controller.OnDespawn();
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
                controller.OnDespawn();
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