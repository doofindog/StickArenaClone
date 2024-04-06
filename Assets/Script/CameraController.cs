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

    public float ppc;
    public Vector3 scrollOffset;
    public PixelPerfectCamera _pixelPerfectCamera;
    
    public void Awake()
    {
        _states.Add(ECameraState.MENU, gameObject.GetComponent<MenuCameraState>());
        _states.Add(ECameraState.GAME, gameObject.GetComponent<FollowCameraState>());

        _pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
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
    
    
    public void LateUpdate()
    {
        Vector3 oldPos = transform.position;

        int x = Mathf.FloorToInt(transform.position.x * (float)ppc);
        int y = Mathf.FloorToInt(transform.position.y * (float)ppc);
        int z = Mathf.FloorToInt(transform.position.z * (float)ppc);

        Vector3 pixelPosition = new Vector3((float)x / (float)ppc, (float)y / (float)ppc, (float)z / (float)ppc);

        scrollOffset = oldPos - pixelPosition;

        transform.position = pixelPosition;
    }
}
