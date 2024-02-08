using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    private static GameManager _instance;
    public static GameManager Singleton => _instance;
    
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private List<SpawnData> spawnLocations;
    private Dictionary<ulong, NetworkObject> _clientObjects = new Dictionary<ulong, NetworkObject>();
    
    public NetworkVariable<float> prepTimer = new NetworkVariable<float>();
    public NetworkVariable<float> startGameTimer = new NetworkVariable<float>();
    
    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        
        
        CustomNetworkEvents.AllPlayersConnectedEvent += StartGameSession;
    }

    private void StartGameSession()
    {
        if (IsServer)
        {
            StartCoroutine(StartGame());
        }
    }

    private IEnumerator StartGame()
    {
        PreparingGameClientRPC();
        while (prepTimer.Value < sessionSettings.prepGameTime)
        {
            yield return new WaitForSeconds(1);
            prepTimer.Value++;
        }

        startGameTimer.Value = sessionSettings.startGameTime;
        StartGameClientRPC();
        while (startGameTimer.Value > 0)
        {
            yield return new WaitForSeconds(1);
            startGameTimer.Value--;
        }
        
        SpawnAllPlayers();
    }

    [ClientRpc]
    private void PreparingGameClientRPC()
    {
        GameEvents.SendPreparingArenaEvent();
    }

    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
    }

    public void AddPlayer(ulong clientId, NetworkObject networkObject)
    {
        _clientObjects ??= new Dictionary<ulong, NetworkObject>();
        if (_clientObjects.ContainsKey(clientId))
        {
            return;
        }

        _clientObjects.Add(clientId, networkObject);
    }

    public NetworkObject GetPlayerNetObject(ulong clientID)
    {
        _clientObjects.TryGetValue(clientID, out NetworkObject netObj);
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
        if (!_clientObjects.TryGetValue(clientID, out NetworkObject networkObject)) return;
        
        
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
        
        if (_clientObjects.TryGetValue(clientId, out NetworkObject obj))
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
        if (_clientObjects.TryGetValue(clientID, out NetworkObject networkObject))
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
        return sessionSettings;
    }

    [ServerRpc(RequireOwnership =false)]
    public void RequestSpawnPlayerServerRPC(ulong clientID)
    {
        RespawnPlayer(clientID);
    }
}