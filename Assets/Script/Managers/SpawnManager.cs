using System;
using System.Linq;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class SpawnManager : NetworkBehaviour
{
    public enum SpawnType
    {
        MONO,
        NETWORK,
    }

    public static SpawnManager Instance { get; private set; }

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<SpawnData> playerSpawnLocations;
    [SerializeField] private ObjectPool _monoObjectPool;
    [SerializeField] private NetworkObjectPool _networkObjectPool;

    private Dictionary<int, GameObject> bulletsFired = new  Dictionary<int, GameObject>();

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void Start()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = playerPrefab;
    }
    
    public GameObject SpawnObject(GameObject prefab, SpawnType spawnType)
    {
        switch (spawnType)
        {
            case SpawnType.MONO:
                return _monoObjectPool.GetPooledObject(prefab, Vector3.zero, Quaternion.identity);
            case SpawnType.NETWORK:
                return _networkObjectPool.GetNetworkObject(prefab, Vector3.zero, Quaternion.identity).gameObject;
            default:
                return null;
        }
    }

    public GameObject SpawnObject(GameObject prefab, SpawnType spawnType, Vector3 position, Quaternion rotation)
    {
        switch (spawnType)
        {
            case SpawnType.MONO:
                return _monoObjectPool.GetPooledObject(prefab, position, rotation);
            case SpawnType.NETWORK:
                return _networkObjectPool.GetNetworkObject(prefab, position, rotation).gameObject;
            default:
                return null;
        }
    }
    
    private Transform GetSpawnLocation(TeamType type)
    {
        return (from location in playerSpawnLocations where type == location.teamType select location.spawnLocation[Random.Range(0, location.spawnLocation.Length)]).FirstOrDefault();
    }
    
    public void SpawnAllPlayers()
    {
        Debug.Log("[GAME SESSION] Spawning Players");
        
        NetworkManager networkManager = NetworkManager.Singleton;
        foreach (ulong clientID in networkManager.ConnectedClients.Keys)
        {
            SpawnPlayer(clientID);
        }
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
    
    [ServerRpc(RequireOwnership =false)]
    public void RequestSpawnPlayerServerRPC(ulong clientID)
    {
        RespawnPlayer(clientID);
    }
    
    [ClientRpc]
    private void RespawnPlayerClientRpc(ulong clientId)
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        PlayerData playerData = connectionManager.GetPlayerData(clientId);
        if (playerData != null)
        {
            
            if (playerData.networkObject != null)
            {
                ClientController clientController = playerData.networkObject.GetComponent<ClientController>();
                if (clientController != null)
                {
                    TeamType playerTeam = TeamManager.Instance.GetTeamFromID(clientId).teamType;
                    Transform spawnTransform = GetSpawnLocation(playerTeam);
                    clientController.transform.position = spawnTransform.position;
                    clientController.OnRespawn();
                }
            }
            else
            {
                Debugger.Log($"[Spawn] Client {clientId} network object is null");
            }

        }
    }

    
    public void DespawnPlayer(ulong clientId)
    {
        if(!IsServer) return;

        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        PlayerData playerData = connectionManager.GetPlayerData(clientId);
        if (playerData != null )
        {
            if (playerData.networkObject != null)
            {
                if(playerData.networkObject.TryGetComponent(out ServerController controller))
                {
                    controller.OnDespawn();
                }
            }
            else
            {
                Debugger.Log($"[Spawn] Client {clientId} network object is null");
            }
        }
        
        SendDespawnClientRpc(clientId);
    }

    [ClientRpc]
    private void SendDespawnClientRpc(ulong clientId)
    {
        
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        PlayerData playerData = connectionManager.GetPlayerData(clientId);
        if (playerData != null)
        {
            if (playerData.networkObject != null)
            {
                ClientController controller = playerData.networkObject.GetComponent<ClientController>();
                if (controller != null)
                {
                    controller.OnDespawn();
                }
            }
            else
            {
                Debugger.Log($"[Spawn] Client {clientId} network object is null");
            }
        }
    }
}
