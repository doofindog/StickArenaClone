using System;
using System.Collections;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class StartScreenCamera : MonoBehaviour
{
    [SerializeField] private float cameraSpeed;
    [SerializeField] private Vector3 networkStartPosition;
    [SerializeField] private Transform[] locations;

    private Transform _followTransform;
    private Vector3 _direction;
    private bool _networkStarted;
    

    private void Awake()
    {
        CustomNetworkEvents.NetworkStartedEvent += MoveCameraToCenter;
        CustomNetworkEvents.DisconnectedEvent += HandleNetworkStopped;
        PlayerEvents.PlayerSpawnedEvent += HandlePlayerConnected;
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

    private void Start()
    {
        UpdateNextLocation();
    }

    private void HandlePlayerConnected(GameObject obj)
    {
        enabled = false;
    }

    private void Update()
    {
        if (!_networkStarted)
        {
            if (Vector2.Distance(transform.position, _followTransform.position) <= 0.1f)
            {
                UpdateNextLocation();
            }
            else
            {
                transform.position += _direction * Time.deltaTime * cameraSpeed;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, networkStartPosition) > 0)
            {
                Vector3 startPosition = Vector3.Lerp(transform.position, networkStartPosition, 0.05f);
                transform.position = startPosition;
            }
        }
    }

    private void UpdateNextLocation()
    {
        if (locations == null || locations.Length <= 0)
        {
            return;
        }

        _followTransform  = locations[Random.Range(0, locations.Length)];
        var followPosition = _followTransform.position;
        followPosition = new Vector3
        (
            followPosition.x,
            followPosition.y,
            -10
        );
        _followTransform.position = followPosition;
        _direction = followPosition - transform.position;
    }
    
    
}
