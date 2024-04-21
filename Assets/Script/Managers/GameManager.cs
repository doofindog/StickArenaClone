using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public NetworkVariable<float> prepTimer = new NetworkVariable<float>();
    public NetworkVariable<float> startGameTimer = new NetworkVariable<float>();

    [Header("Managers")]
    public ConnectionManager connectionManager;
    public SpawnManager spawnManager;
    public TickManager tickManager;
    public TeamManager teamManager;
    public ArenaManager arenaManger;
    public ScoreManager scoreManager;
    
    [SerializeField] private SessionSettings sessionSettings;
    
    private BaseGameState _currentState;
    private EGameStates _currentStateType;
    private Dictionary<EGameStates, BaseGameState> _gameStates = new Dictionary<EGameStates, BaseGameState>();
    
    
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
    
    public async void Start()
    {
        await Initialise();
    }
    

    private async Task Initialise()
    {
        //Cursor.visible = false;
        
        _gameStates.Add(EGameStates.MENU, GetComponent<MenuState>());
        _gameStates.Add(EGameStates.GAME, GetComponent<GameState>());
        _gameStates.Add(EGameStates.OVER, GetComponent<GameOverState>());
        
        await connectionManager.Init();
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



    public SessionSettings GetSessionSettings()
    {
        return sessionSettings;
    }
    
    public async void TryJoin(string username, string joinCode = "")
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await connectionManager.TrySignInPlayer(username);
        }
        
        JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        NetworkManager.Singleton.StartClient();
        
        CustomNetworkEvents.SendNetworkStartedEvent();
    }

    public async void TryStartHost(string username)
    {
        CustomNetworkEvents.SendNetworkStartedEvent();
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await connectionManager.TrySignInPlayer(username);
        }
#if UNITY_EDITOR
        List<Region> regions = await RelayService.Instance.ListRegionsAsync();

        foreach (var region in regions)
        {
            Debug.Log($"[CONNECTION] : Region : {region.Description} :  {region.Id}");
        }
#endif

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(connectionManager.MaxPlayers);
        
        
        ConnectionPayload connectionPayload = new ConnectionPayload() { userName = username };
        string payloadJson = JsonUtility.ToJson(connectionPayload);
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(payloadJson);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log("[CONNECTION] join Code : " + joinCode);
        NetworkManager.Singleton.StartHost();
        
        
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