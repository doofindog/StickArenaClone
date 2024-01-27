using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class GameSpawnManager : NetworkSingleton<GameSpawnManager>
{
    [SerializeField] private Transform[] spawnPoints;
    public NetworkObjectPool networkObjectPool;
    public ObjectPool objectPool;
    
    private NetworkVariable<NetworkObject[]> _spawnedPlayers;
    
    protected override void Awake()
    {
        base.Awake();
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            GameEvents.GameSessionStartedEvent += SpawnAllPlayers;
        }
    }
    
    
    private void SpawnPlayer(ulong clientID)
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        GameObject playerPrefab = Instantiate(networkManager.NetworkConfig.PlayerPrefab);
        playerPrefab.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        playerPrefab.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientID);
    }
    
    private void SpawnAllPlayers()
    {
        NetworkManager networkManager = NetworkManager.Singleton;
        foreach (ulong clientID in networkManager.ConnectedClients.Keys)
        {
            Debug.Log("Client IDs" + clientID);
            SpawnPlayer(clientID);
        }
    }

    private void DespawnPlayer(ulong intID)
    {
        
    }
}