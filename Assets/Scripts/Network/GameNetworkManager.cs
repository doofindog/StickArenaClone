using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class GameNetworkManager : Singleton<GameNetworkManager>
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TickManager _tickManager;
    [FormerlySerializedAs("_connectionManager")] [SerializeField] private LobbyManager lobbyManager;

    public void Awake()
    {
        _networkManager = _networkManager == null ? GetComponent<NetworkManager>() : _networkManager;
        _tickManager = _tickManager == null ? GetComponent<TickManager>() : _tickManager;
        lobbyManager = lobbyManager == null ? GetComponentInChildren<LobbyManager>() : lobbyManager;

        _networkManager.OnClientStarted += OnNetworkStart;
        _networkManager.OnServerStarted += OnNetworkStart;
    }

    private void OnNetworkStart()
    {
        _tickManager.Init(true);
        lobbyManager.Init();
    }

    public NetworkManager GetNetworkManager()
    {
        return _networkManager;
    }

    public TickManager GetTickManager()
    {
        return _tickManager;
    }

    public LobbyManager GetConnectionManager()
    {
        return lobbyManager;
    }
}
