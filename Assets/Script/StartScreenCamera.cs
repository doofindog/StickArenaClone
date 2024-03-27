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
    public float ppc;
    public Vector3 scrollOffset;
    

    private void Awake()
    {
        CustomNetworkEvents.NetworkStartedEvent += MoveCameraToCenter;
        CustomNetworkEvents.DisconnectedEvent += HandleNetworkStopped;
        PlayerEvents.PlayerSpawnedEvent += HandlePlayerConnected;
    }

    private void Start()
    {
        UpdateNextLocation();
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
                _direction = _followTransform.position - transform.position;
                transform.position += _direction.normalized * Time.deltaTime * cameraSpeed + scrollOffset;
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


    private void LateUpdate()
    {
        Vector3 oldPos = transform.position;

        int i_x = Mathf.FloorToInt(transform.position.x * (float)ppc);
        int i_y = Mathf.FloorToInt(transform.position.y * (float)ppc);
        int i_z = Mathf.FloorToInt(transform.position.z * (float)ppc);

        Vector3 p = new Vector3((float)i_x / (float)ppc, (float)i_y / (float)ppc, (float)i_z / (float)ppc);

        scrollOffset = oldPos - p;

        transform.position = p;

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
        _direction = _followTransform.position - transform.position;
    }    
}
