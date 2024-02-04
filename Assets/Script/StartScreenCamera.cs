using System;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class StartScreenCamera : MonoBehaviour
{

    [SerializeField] private PixelPerfectCamera _pixelPerfectCamera;
    [SerializeField] private float _cameraSpeed;
    [SerializeField] private Vector3 _networkStartPosition;
    [SerializeField] private Transform[] _locations;

    private Transform _followTransform;
    private Vector3 _direction;
    private bool _networkStarted;

    private void Awake()
    {
        _pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        CustomNetworkEvents.NetworkStartedEvent += MoveCameraToCenter;
    }

    private void Start()
    {
        UpdateNextLocation();
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
                transform.position += _direction * Time.deltaTime * _cameraSpeed;
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, _networkStartPosition) > 0)
            {
                Vector3 startPosition = Vector3.Lerp(transform.position, _networkStartPosition, 0.005f);
                transform.position = startPosition;
            }
        }
    }

    private void UpdateNextLocation()
    {
        if (_locations == null || _locations.Length <= 0)
        {
            return;
        }

        _followTransform  = _locations[Random.Range(0, _locations.Length)];
        _followTransform.position = new Vector3
        (
            _followTransform.position.x,
            _followTransform.position.y,
            -10
        );
        _direction = _followTransform.position - transform.position;
    }

    private void MoveCameraToCenter()
    {
        Debug.Log("Called");
        _networkStarted = true;
    }
}
