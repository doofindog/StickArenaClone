using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class GameNetworkManager : Singleton<GameNetworkManager>
{
    [SerializeField] private NetworkManager _networkManager;
    [SerializeField] private TickManager _tickManager;
    [SerializeField] private LobbyManager _lobbyManager;

    protected override void Awake()
    {
        base.Awake();
        
        _networkManager = _networkManager == null ? GetComponent<NetworkManager>() : _networkManager;
        _tickManager = _tickManager == null ? GetComponentInChildren<TickManager>() : _tickManager;
        _lobbyManager = _lobbyManager == null ? GetComponentInChildren<LobbyManager>() : _lobbyManager;

        _networkManager.OnServerStarted += OnNetworkStart;
        _networkManager.OnClientStarted += OnNetworkStart;
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

    public LobbyManager GetConnectionManager()
    {
        return _lobbyManager;
    }
}
