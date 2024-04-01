using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;

public class FollowCameraState : CameraState, ITickableEntity
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _interpolationSpeed;
    
    [Header("Shake")] 
    [SerializeField] private float _positionShakeIntensity;
    [SerializeField] private float _rotationShakeIntensity;
    [SerializeField] private float _shakeDuration;
    private float _shakeTimer;
    private bool _performShake;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    

    public override void Enter()
    {
        PlayerEvents.PlayerDiedEvent += PerformShake;
        GameEvents.WeaponFiredEvent += PerformShake;
        
        NetworkSpawnManager spawnManager = NetworkManager.Singleton.SpawnManager;
        ulong playerId = NetworkManager.Singleton.LocalClientId;
        if (spawnManager.GetPlayerNetworkObject(playerId) != null)
        {
            Debug.Log("Called");
            _follow = spawnManager.GetPlayerNetworkObject(playerId).gameObject.transform;
        }
        else
        {
            Debug.Log("No player Object found");
        }
        
        TickManager.Instance.AddEntity(this);
        
        
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    public override void UpdateState()
    {
        if (_performShake)
        {
            Shake();
        }
        else
        {
            _originalPosition = transform.position + CameraController.Instance.scrollOffset;
            _originalRotation = transform.rotation;
        }
    }

    public override void Exit()
    {
        PlayerEvents.PlayerDiedEvent -= PerformShake;
        GameEvents.WeaponFiredEvent -= PerformShake;
        
        TickManager.Instance.AddEntity(this);
    }
    
    
    
    private void PerformShake()
    {
        _performShake = true;
    }

    private void Shake()
    {
        if (_shakeTimer < _shakeDuration )
        {
            // Calculate Perlin noise values for smooth randomness
            float perlinX = Mathf.PerlinNoise(Time.time * 10f, 0f) * 2 - 1;
            float perlinY = Mathf.PerlinNoise(0f, Time.time * 10f) * 2 - 1;
            float perlinRotZ = Mathf.PerlinNoise(0f, Time.time * 10f) * 2 - 1;

            // Calculate the shake offset for position and rotation using Perlin noise and intensity
            Vector3 positionShakeOffset = new Vector3(perlinX, perlinY, 0f) * _positionShakeIntensity;
            Vector3 rotationShakeOffset = new Vector3(0f, 0f, perlinRotZ) * _rotationShakeIntensity;

            // Apply the shake offset to the camera position and rotation
            transform.position = _originalPosition + positionShakeOffset + _cameraOffset + CameraController.Instance.scrollOffset;
            transform.rotation = _originalRotation * Quaternion.Euler(rotationShakeOffset);

            // Increment the elapsed time
            _shakeTimer += Time.deltaTime;
        }
        else
        {
            // Reset the camera position and rotation after the shake duration
            transform.position = _originalPosition + CameraController.Instance.scrollOffset;
            transform.rotation = _originalRotation;
            _shakeTimer = 0;
            _performShake = false;
        }
    }

    public void UpdateTick(int tick)
    {
        if(_follow == null) return;

        Vector3 followPosition = _follow.position + _cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, followPosition,
            _interpolationSpeed * TickManager.Instance.GetMinTickTime());

        _originalPosition = smoothedPosition;
        transform.position = smoothedPosition + CameraController.Instance.scrollOffset;
    }
}
