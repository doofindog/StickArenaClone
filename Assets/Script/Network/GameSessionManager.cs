using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameSessionManager : NetworkBehaviour
{
    [SerializeField] private GameObject testWeaponPrefab;
    [SerializeField] private Transform[] spawnPoints;

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
            SpawnPlayer(clientID);
        }
    }
}