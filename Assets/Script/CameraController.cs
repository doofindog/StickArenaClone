using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public enum ECameraState
{
    MENU,
    GAME,
    OVER,
}

public class CameraController : Singleton<CameraController>
{
    
    [SerializeField] private CameraState _currentState;
    private Dictionary<ECameraState,CameraState> _states = new Dictionary<ECameraState,CameraState>();
    
    public void Awake()
    {
        _states.Add(ECameraState.MENU, gameObject.GetComponent<MenuCameraState>());
        _states.Add(ECameraState.GAME, gameObject.GetComponent<FollowCameraState>());
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
