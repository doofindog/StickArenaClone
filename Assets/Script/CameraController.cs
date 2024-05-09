using System.Collections.Generic;
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
    
    [SerializeField] private CameraState _currentState;
    [SerializeField] private Camera uiCamera;
    private Dictionary<ECameraState,CameraState> _states = new Dictionary<ECameraState,CameraState>();
    
    public void Awake()
    {
        _states.Add(ECameraState.MENU, gameObject.GetComponent<MenuCameraState>());
        _states.Add(ECameraState.GAME, gameObject.GetComponent<FollowCameraState>());

        GameEvents.TeamWonEvent += OnTeamWon;
    }

    private void OnTeamWon(TeamType obj)
    {
        Debug.Log("Called");
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
        if (_currentState != null)
        {
            _currentState.UpdateState();
        }
    }

    public void ChangeState(ECameraState state)
    {
        if (_currentState != null) {
            _currentState.Exit();
        }
        
        _currentState = _states[state];

        if (_currentState != null) {
            _currentState.Enter();
        }
    }
}
