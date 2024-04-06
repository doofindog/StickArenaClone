using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public NetworkVariable<float> prepTimer = new NetworkVariable<float>();
    public NetworkVariable<float> startGameTimer = new NetworkVariable<float>();

    [Header("Managers")]
    public ConnectionManager connectionManager;
    public TickManager tickManager;
    public TeamManager teamManager;
    public ArenaManager arenaManger;
    public ScoreManager scoreManager;
    
    [SerializeField] private SessionSettings sessionSettings;
    [SerializeField] private List<SpawnData> spawnLocations;
    
    private BaseGameState _currentState;
    private EGameStates _currentStateType;
    private Dictionary<EGameStates, BaseGameState> _gameStates = new Dictionary<EGameStates, BaseGameState>();
    private Dictionary<ulong, NetworkObject> _clientObjects = new Dictionary<ulong, NetworkObject>();
    
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        
    }
    
    public void Start()
    {
        Initialise();
    }
    

    private void Initialise()
    {
        //Cursor.visible = false;
        
        _gameStates.Add(EGameStates.MENU, GetComponent<MenuState>());
        _gameStates.Add(EGameStates.GAME, GetComponent<GameState>());
        _gameStates.Add(EGameStates.OVER, GetComponent<EndState>());
        
        connectionManager.Init();
        tickManager.Init();
        teamManager.Init();
        arenaManger.Init();
        
        SwitchState(EGameStates.MENU);
    }

    public void SwitchState(EGameStates state)
    {
        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        _currentStateType = state;
        _currentState = GetGameState(state);
        _currentState.OnEnter();
    }
    
    public EGameStates GetState()
    {
        return _currentStateType;
    }

    private BaseGameState GetGameState(EGameStates state)
    {
        _gameStates.TryGetValue(state, out BaseGameState gameState);
        return gameState;
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

    public void SpawnAllPlayers()
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
    
    public void TryJoin(string username)
    {
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.StartClient();
        
        CustomNetworkEvents.SendNetworkStartedEvent();
    }

    public void TryStartHost(string username)
    {
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.StartHost();
        
        CustomNetworkEvents.SendNetworkStartedEvent();
    }
    
    public void TryDisconnect()
    {
        connectionManager.ClearData();
        NetworkManager.Singleton.Shutdown();
        TeamManager.Instance.Reset();

        if (IsServer)
        {
            prepTimer.Value = 0;
            startGameTimer.Value = 0;
        }

        CustomNetworkEvents.SendDisconnectedEvent();
    }
}