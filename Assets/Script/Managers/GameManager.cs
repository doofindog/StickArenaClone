using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using Unity.Netcode;

using PixelArena.UI;

#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class GameManager : NetworkBehaviour
{
    public enum GameType
    {
        SERVER,
        CLIENT
    }

    public static GameManager Instance { get; private set; }

    public NetworkVariable<float> prepTimer = new NetworkVariable<float>();
    public NetworkVariable<float> startGameTimer = new NetworkVariable<float>();

    [Header("Managers")]
    public ConnectionManager connectionManager;
    public SpawnManager spawnManager;
    public TickManager tickManager;
    public TeamManager teamManager;
    public ScoreManager scoreManager;
    public SessionManager sessionManager;
    public UIManager uIManager;

    [Header("Settings")]
    public GameSettings sessionSettings;

    private BaseGameState m_currentState;
    private EGameStates m_currentStateType;
    private Dictionary<EGameStates, BaseGameState> m_gameStates = new Dictionary<EGameStates, BaseGameState>();

#if UNITY_EDITOR
    private GameType m_gameType;
#endif


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
        SessionManager.AllPlayersConnectedEvent += StartGameSession;

        Cursor.visible = false;

        m_gameStates.Add(EGameStates.MENU, GetComponentInChildren<MenuState>());
        m_gameStates.Add(EGameStates.GAME, GetComponentInChildren<GameState>());
        m_gameStates.Add(EGameStates.OVER, GetComponentInChildren<GameOverState>());

        connectionManager ??= ConnectionManager.Instance;
        tickManager ??= TickManager.Instance;
        teamManager ??= TeamManager.Instance;
        sessionManager ??= SessionManager.Instance;
        uIManager ??= UIManager.Instance;

        connectionManager.Init();
        tickManager.Init();
        teamManager.Init();
        sessionManager.Init();
        uIManager.Init();

        void HandleServer()
        {
            m_gameType = GameType.SERVER;
            connectionManager.StartServer();
        }

        void HandleClient()
        {
            m_gameType = GameType.CLIENT;
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);

        SwitchState(EGameStates.MENU);
    }

    public void StartGameSession()
    {
        void HandleServer()
        {
            SwitchState(EGameStates.GAME, 3);
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, null);
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

        if(IsServer)
        {
            SwitchStateClientRPC(state, delay);
        }
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

    #region RPCS

    [ClientRpc]
    public void SwitchStateClientRPC(EGameStates pGameState, int delay = 0)
    {
        SwitchState(pGameState, delay);
    }

    #endregion


#if UNITY_EDITOR
    public GameType GetGameType()
    {
        return m_gameType;
    }

#endif
}