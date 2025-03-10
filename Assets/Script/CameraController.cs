using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

public enum ECameraState
{
    MENU,
    GAME,
    OVER,
}

public class CameraController : Singleton<CameraController>
{
    [SerializeField] private Camera uiCamera;

    private CameraState m_currentState;
    private Dictionary<ECameraState,CameraState> m_states = new Dictionary<ECameraState,CameraState>();

    public Transform target;

    protected override void Awake()
    {
        base.Awake();
        void HandleServer()
        {
            this.enabled = false;
        }

        void HandleClient()
        {
            m_states.Add(ECameraState.MENU, gameObject.GetComponent<MenuCameraState>());
            m_states.Add(ECameraState.GAME, gameObject.GetComponent<FollowCameraState>());

            foreach(CameraState state in m_states.Values)
            {
                state.Init(this);
            }

            GameEvents.TeamWonEvent += OnTeamWon;
            GameEvents.OnGameStartEvent += () =>
            {
                ChangeState(ECameraState.GAME);
            };
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    private void OnTeamWon(TeamType obj)
    {
        UniversalAdditionalCameraData cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = false;
        cameraData.cameraStack.Add(uiCamera);
    }

    public void Start()
    {
        ChangeState(ECameraState.MENU);
    }
    
    public void Update()
    {
        if (m_currentState != null)
        {
            m_currentState.UpdateState();
        }
    }

    public void ChangeState(ECameraState state)
    {
        if (m_currentState != null) {
            m_currentState.Exit();
        }
        
        m_currentState = m_states[state];

        if (m_currentState != null) {
            m_currentState.Enter();
        }
    }

    public void SetCameraTarget(Transform pTarget)
    {
        target = pTarget;
    }
}
