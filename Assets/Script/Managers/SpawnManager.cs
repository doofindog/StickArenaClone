using System;
using System.Linq;
using System.Collections.Generic;
using Mono.CSharp;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEditor;
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
    [SerializeField] private ObjectPool _monoObjectPool;
    [SerializeField] private NetworkObjectPool _networkObjectPool;
    [SerializeField] private GameObject defaultWeapon;
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
        defaultWeapon = GameManager.Instance.GetSessionSettings().defaultWeapon;
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
    
    private Vector3 GetSpawnLocation(TeamType type)
    {
        switch (type)
        {
            case TeamType.Blue:
            {
                    return new Vector3()
                    {
                        x = Random.Range(-5.0f, 0.0f),
                        y = Random.Range(0.0f, 5.0f)
                    };
            }
            case TeamType.Red:
                {
                    return new Vector3()
                    {
                        x = Random.Range(0.0f, 5.0f),
                        y = Random.Range(-5.0f, 0.0f)
                    };
                }
            default:
                return Vector3.zero;
        }
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
        playerObj.transform.position = GetSpawnLocation(playerTeam);
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }
    
    private void RespawnPlayer(ulong clientID)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientID);
        ServerController serverController = networkObject.GetComponent<ServerController>();
        TeamType playerTeam = TeamManager.Instance.GetTeamFromID(clientID).teamType;
        serverController.transform.position = GetSpawnLocation(playerTeam);
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
        ClientData playerData = SessionManager.Instance.GetClientData(clientId);
        if (!playerData.isNull)
        {
            return;
        }

        if (playerData.networkObject != null)
        {
            ClientController clientController = playerData.networkObject.GetComponent<ClientController>();
            if (clientController != null)
            {
                TeamType playerTeam = TeamManager.Instance.GetTeamFromID(clientId).teamType;
                Vector3 spawnPosition = GetSpawnLocation(playerTeam);
                clientController.transform.position = spawnPosition;
                clientController.OnRespawn();
            }
        }
        else
        {
            Debugger.Log($"[Spawn] Client {clientId} network object is null");
        }
    }
    
    //public void DespawnPlayer(ulong clientId)
    //{
    //    if(!IsServer) return;

    //    ConnectionManager connectionManager = GameManager.Instance.connectionManager;
    //    ClientData clientData = SessionManager.Instance.GetClientData(clientId);
    //    if (!clientData.isNull)
    //    {
    //        if (clientData.networkObject != null)
    //        {
    //            if(clientData.networkObject.TryGetComponent(out ServerController controller))
    //            {
    //                controller.OnDespawn();
    //            }
    //        }
    //        else
    //        {
    //            Debugger.Log($"[Spawn] Client {clientId} network object is null");
    //        }
    //    }
        
    //    SendDespawnClientRpc(clientId);
    //}

    //[ClientRpc]
    //private void SendDespawnClientRpc(ulong clientId)
    //{
    //    ConnectionManager connectionManager = GameManager.Instance.connectionManager;
    //    ClientData playerData = SessionManager.Instance.GetClientData(clientId);
    //    if (!playerData.isNull || playerData.networkObject != null)
    //    {
    //        return;
    //    }

    //    ClientController controller = playerData.networkObject.GetComponent<ClientController>();
    //    if (controller != null)
    //    {
    //        controller.OnDespawn();
    //    }
    //}
}
