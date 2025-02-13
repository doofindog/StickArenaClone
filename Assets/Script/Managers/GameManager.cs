using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Serialization;

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
    public SessionManager sessionManager;

    [Header("Settings")]
    public bool startServerOnBoot;
    public GameSettings sessionSettings;

    private BaseGameState m_currentState;
    private EGameStates m_currentStateType;
    private Dictionary<EGameStates, BaseGameState> m_gameStates = new Dictionary<EGameStates, BaseGameState>();


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
        Cursor.visible = false;

        m_gameStates.Add(EGameStates.MENU, GetComponent<MenuState>());
        m_gameStates.Add(EGameStates.GAME, GetComponent<GameState>());
        m_gameStates.Add(EGameStates.OVER, GetComponent<GameOverState>());


        connectionManager ??= ConnectionManager.Instance;
        tickManager ??= TickManager.Instance;
        teamManager ??= TeamManager.Instance;
        arenaManger ??= ArenaManager.Instance;
        sessionManager ??= SessionManager.Instance;

        connectionManager.Init();
        tickManager.Init();
        teamManager.Init();
        arenaManger.Init();
        sessionManager.Init();

        SwitchState(EGameStates.MENU);
    }

    public void SwitchState(EGameStates state, int delay = 0)
    {
        if(delay != 0)
        {
            StartCoroutine(SwitchStateDelayed(delay, state));
            return;
        }

        if (m_currentState != null)
        {
            m_currentState.OnExit();
        }

        m_currentStateType = state;
        m_currentState = GetGameState(state);
        m_currentState.OnEnter();

        Debugger.Log("[GAME_MANAGER] Game State has been Changed");
        GameEvents.SendGameStateChange(state);
    }

    private IEnumerator SwitchStateDelayed(int delay, EGameStates state)
    {
        yield return new WaitForSeconds(delay);

        SwitchState(state);
    }

    public EGameStates GetState()
    {
        return m_currentStateType;
    }

    private BaseGameState GetGameState(EGameStates state)
    {
        m_gameStates.TryGetValue(state, out BaseGameState gameState);
        return gameState;
    }

    public GameSettings GetSessionSettings()
    {
        return sessionSettings;
    }
}