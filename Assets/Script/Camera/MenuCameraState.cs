using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MenuCameraState : CameraState
{
    [SerializeField] private float _cameraSpeed;
    [SerializeField] private Vector3 _networkStartPosition;
    [SerializeField] private Transform[] _locations;

    private Transform _followTransform;
    private Vector3 _direction;
    private bool _networkStarted;
    
    public override void Enter()
    {
        CustomNetworkEvents.NetworkStartedEvent += MoveCameraToCenter;
        CustomNetworkEvents.DisconnectedEvent += HandleNetworkStopped;
        PlayerEvents.PlayerSpawnedEvent += HandlePlayerConnected;
        GameEvents.PreparingArenaEvent += EnableUpscaling;
        
        UpdateNextLocation();
    }

    public override void UpdateState()
    {
        if (!_networkStarted)
        {
            if(_followTransform == null) return;

            if (Vector2.Distance(transform.position, _followTransform.position) <= 0.1f)
            {
                UpdateNextLocation();
            }
            else
            {
                _direction = _followTransform.position - transform.position;
                transform.position += (_direction.normalized * (Time.deltaTime * _cameraSpeed)) + 
                                      CameraController.Instance.scrollOffset;
            }
        }
        else
        {
            if (!(Vector3.Distance(transform.position, _networkStartPosition) > 0)) return;
            
            Vector3 startPosition = Vector3.Lerp(transform.position, _networkStartPosition, 0.05f);
            transform.position = startPosition;
        }
    }

    public override void Exit()
    {
        CustomNetworkEvents.NetworkStartedEvent -= MoveCameraToCenter;
        CustomNetworkEvents.DisconnectedEvent -= HandleNetworkStopped;
        PlayerEvents.PlayerSpawnedEvent -= HandlePlayerConnected;
    }
    
    
    private void MoveCameraToCenter()
    {
        _networkStarted = true;
    }

    private void HandleNetworkStopped()
    {
        UpdateNextLocation();
        _networkStarted = false;
    }

    private void HandlePlayerConnected(GameObject obj)
    {
        CameraController.Instance.ChangeState(ECameraState.GAME);
    }

    private void UpdateNextLocation()
    {
        if (_locations == null || _locations.Length <= 0) return;

        _followTransform  = _locations[Random.Range(0, _locations.Length)];
        Vector3 followPosition = _followTransform.position;
        followPosition = new Vector3
        (
            followPosition.x,
            followPosition.y,
            -10
        );
        
        _followTransform.position = followPosition;
        _direction = _followTransform.position - transform.position;
    }


    private void EnableUpscaling()
    {
        if (CameraController.Instance._pixelPerfectCamera != null)
        {
            CameraController.Instance._pixelPerfectCamera.gridSnapping =
                UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
        }
    }
}
