using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameNetworkManager : Singleton<GameNetworkManager>
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TickManager _tickManager;
    [SerializeField] private ConnectionManager _connectionManager;
    
    protected override void Awake()
    {
        base.Awake();
        
        _networkManager.OnServerStarted += OnNetworkStart;
        _networkManager.OnClientStarted += OnNetworkStart;
        CustomNetworkEvents.AllPlayersConnectedEvent += StartGame;
    }

    protected void OnDestroy()
    {
        _networkManager.OnServerStarted -= OnNetworkStart;
        _networkManager.OnClientStarted -= OnNetworkStart;
        CustomNetworkEvents.AllPlayersConnectedEvent -= StartGame;
    }

    private void OnNetworkStart()
    {
        _tickManager.Init();
    }

    public NetworkManager GetNetworkManager()
    {
        return _networkManager;
    }

    public TickManager GetTickManager()
    {
        return _tickManager;
    }

    public ConnectionManager GetConnectionManager()
    {
        return _connectionManager;
    }


    private void StartGame()
    {
        StartCoroutine(LoadGame());
    }

    private IEnumerator LoadGame()
    {
        yield return new WaitForSeconds(3);
        
        NetworkSceneManager sceneManager = NetworkManager.Singleton.SceneManager;
        sceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
    }
}
