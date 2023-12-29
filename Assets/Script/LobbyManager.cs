using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Mathematics;
using Unity.Multiplayer.Playmode;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class LobbyManager : Singleton<LobbyManager>
{
    private const string LOBBY_NAME_PREFIX = "CARGO_LOBBY_";
    private const int MAX_PLAYERS = 6;

    [SerializeField] private string _playerUserName;
    [SerializeField] private string _lobbyIdText;
    [SerializeField] private float _lobbyUpdateTimer;
    [SerializeField] private float _lobbyUpdateRate;
    [SerializeField] private GameObject testWeaponPrefab;
    [SerializeField] private List<NetworkObject> clientNetworkObjects = new List<NetworkObject>();

    private bool _lobbyJoined;
    private Player _player;
    private Lobby _currentLobbyState;

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
#if UNITY_EDITOR
        _playerUserName = GUILayout.TextField(CurrentPlayer.ReadOnlyTags().First());  
#else 
        _playerUserName = GUILayout.TextField(_playerUserName); 
#endif
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        _lobbyIdText = GUILayout.TextField(_lobbyIdText, new []{GUILayout.MinWidth(50)});
        if (GUILayout.Button("Join Lobby")) {  TryJoinLobby(_lobbyIdText);}
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Create lobby"))
        {
            CreateLobby();
        }
    }

    private void Update()
    {
        _lobbyUpdateTimer -= Time.deltaTime;
        if (_lobbyUpdateTimer <= 0.0f)
        {
            UpdateLobby();
            _lobbyUpdateTimer = _lobbyUpdateRate;
        }
        
    }

    private async void CreateLobby()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
            
            _player = await TrySignInPlayer();
            _currentLobbyState = await LobbyService.Instance.CreateLobbyAsync
                (
                    lobbyName: LOBBY_NAME_PREFIX + Guid.NewGuid(),
                    maxPlayers: MAX_PLAYERS,
                    options: new CreateLobbyOptions()
                    {
                        IsPrivate = false,
                    }
                );
            
            Debug.Log("[Lobby] Lobby Created :" + _currentLobbyState.Id);
            StartCoroutine(PerformHeartBeatToLobby());
        }
        catch (Exception e)
        {
            Debug.Log("Exception :" + e);
            throw;
        }
    }

    public async void TryJoinLobby(string lobbyID)
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }
            
            _player = await TrySignInPlayer();
            JoinLobbyByIdOptions joinOption = new JoinLobbyByIdOptions
            {
                Player = _player
            };
            _currentLobbyState = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyID, joinOption);
            if (_currentLobbyState == null)
            {
                throw new Exception("[Join Lobby] could not join lobby");
            }

            _currentLobbyState = await LobbyService.Instance.UpdatePlayerAsync
            (
                lobbyId: _currentLobbyState.Id,
                playerId: _player.Id,
                new UpdatePlayerOptions()
                {
                    
                }         
            );
        }
        catch (Exception e)
        {
            Debug.LogError("[Join Lobby] " + e);
        }
    }
    
    private async void UpdateLobby()
    {
        if (_currentLobbyState == null)
        {
            return;
        }
        _currentLobbyState = await LobbyService.Instance.GetLobbyAsync(_currentLobbyState.Id);
    }
    
    private async Task<Player> TrySignInPlayer()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
#if UNITY_EDITOR
            //AuthenticationService.Instance.ClearSessionToken(); 
#endif
            AuthenticationService.Instance.SwitchProfile(_playerUserName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                throw new Exception("Player was not signed in Successfully");
            }
        }
        
        Debug.Log("[Authentication] Player ID " + AuthenticationService.Instance.PlayerId);
        return new Player(AuthenticationService.Instance.PlayerId, null, new Dictionary<string, PlayerDataObject>());
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

    private IEnumerator PerformHeartBeatToLobby()
    {
        if (_currentLobbyState == null)
        {
            yield break;
        }

        while (true)
        {
            try
            {
                LobbyService.Instance.SendHeartbeatPingAsync(_currentLobbyState.Id);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            yield return new WaitForSeconds(25);
        }
        
    }

    [Command]
    private async void LogLobbyData()
    {
        if(_currentLobbyState == null) return;
        
        _currentLobbyState = await LobbyService.Instance.GetLobbyAsync(_currentLobbyState.Id);
        foreach (Player player in _currentLobbyState.Players)
        {
            Debug.Log("Player :" + player.Id);
        } 
    }
}