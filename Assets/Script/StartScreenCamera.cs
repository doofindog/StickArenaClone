using System;
using System.Collections;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

public class StartScreenCamera : MonoBehaviour
{

    [SerializeField] private UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera _pixelPerfectCamera;
    [SerializeField] private float _cameraSpeed;
    [SerializeField] private Vector3 _networkStartPosition;
    [SerializeField] private Transform[] _locations;

    private Transform _followTransform;
    private Vector3 _direction;
    private bool _networkStarted;
    

    private void Awake()
    {
        _pixelPerfectCamera = GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>();
        CustomNetworkEvents.NetworkStartedEvent += MoveCameraToCenter;
        GameEvents.StartGameEvent += HandleOnGameStarted;
        PlayerEvents.PlayerSpawnedEvent += HandlePlayerConnected;
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
        _networkStarted = true;
    }

    private void HandleOnGameStarted()
    {
        StartCoroutine(RemovedCropCameraFrame());
    }

    private IEnumerator RemovedCropCameraFrame()
    {
        float elapsedTime = 0.0f;
        float duration = GameManager.Singleton.GetSessionSettings().startGameTime;
        while (elapsedTime < duration)
        {
            int pixelY = (int)Mathf.Lerp(130, 180, elapsedTime / duration);
            _pixelPerfectCamera.refResolutionY = pixelY;
            elapsedTime+=Time.deltaTime;
            yield return null;
        }

        _pixelPerfectCamera.refResolutionY = 180;
    }
}
