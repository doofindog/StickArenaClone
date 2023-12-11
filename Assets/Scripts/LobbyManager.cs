using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class LobbyManager : Singleton<LobbyManager>
{
  
    [SerializeField] private GameObject testWeaponPrefab;
    [SerializeField] private NetworkObject owningNetworkObj;
    [SerializeField] private List<NetworkObject> clientNetworkObjects = new List<NetworkObject>();

    public void Init()
    {
        NetworkManager networkManager = GameNetworkManager.Instance.GetNetworkManager();
        networkManager.OnClientConnectedCallback += OnPlayerConnected;
        networkManager.OnClientStarted += HandleClientStarted;
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            StartButton();
        }

        GUILayout.EndArea();
    }

    void StartButton()
    {
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }

    private void SetOwningPlayer(GameObject obj)
    {
        obj.TryGetComponent<NetworkObject>(out owningNetworkObj);
    }

    public NetworkObject GetOwningPlayer()
    {
        return owningNetworkObj;
    }

    private void HandleClientStarted()
    {
        NetworkManager networkManager = GameNetworkManager.Instance.GetNetworkManager();
    }

    private void OnPlayerConnected(ulong clientID)
    {
        NetworkSpawnManager spawnManager = GameNetworkManager.Instance.GetNetworkManager().SpawnManager;
        NetworkObject clientObj = spawnManager.GetPlayerNetworkObject(clientID);
        clientNetworkObjects.Add(clientObj);

        if (NetworkManager.Singleton.IsServer)
        {
            float x = Random.Range(-3.0f, +3.0f);
            float y = Random.Range(-2.0f, +2.0f);
            Vector3 randomPosition = new Vector3(x, y, 0);
            GameObject weaponObject = Instantiate(testWeaponPrefab, randomPosition, quaternion.identity);
            weaponObject.GetComponent<NetworkObject>().Spawn();
        }
    }
}