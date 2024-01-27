using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCamera : MonoBehaviour, ITickableEntity
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _interpolationSpeed;


    [Header("Shake")] 
    [SerializeField] private float positionShakeIntensity;
    [SerializeField] private float rotationShakeIntensity;
    [SerializeField] private float shakeDuration;
    private float _shakeTimer;
    private bool _performShake;
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    public void Awake()
    {
        PlayerEvents.PlayerSpawnedEvent += HandlePlayerConnected;
        PlayerEvents.PlayerDiedEvent += PerformShake;
        GameEvents.WeaponFiredEvent += PerformShake;
    }

    public void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    private void HandlePlayerConnected(GameObject obj)
    {
        NetworkSpawnManager spawnManager = NetworkManager.Singleton.SpawnManager;
        ulong playerId = NetworkManager.Singleton.LocalClientId;
        if (spawnManager.GetPlayerNetworkObject(playerId) != null)
        {
            _follow = spawnManager.GetPlayerNetworkObject(playerId).gameObject.transform;
        }
        else
        {
            Debug.Log("No player Object found");
        }
        
        TickManager.Instance.AddEntity(this);
    }

    public void UpdateTick(int tick)
    {
        if(_follow == null) return;

        Vector3 followPosition = _follow.position + _cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, followPosition,
            _interpolationSpeed * TickManager.Instance.GetMinTickTime());

        transform.position = smoothedPosition;
    }

    public void Update()
    {
        if (_performShake)
        {
            Shake();
        }
        else
        {
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }
    }

    private void PerformShake()
    {
        _performShake = true;
    }

    private void Shake()
    {
        if (_shakeTimer < shakeDuration )
        {
            // Calculate Perlin noise values for smooth randomness
            float perlinX = Mathf.PerlinNoise(Time.time * 10f, 0f) * 2 - 1;
            float perlinY = Mathf.PerlinNoise(0f, Time.time * 10f) * 2 - 1;
            float perlinRotZ = Mathf.PerlinNoise(0f, Time.time * 10f) * 2 - 1;

            // Calculate the shake offset for position and rotation using Perlin noise and intensity
            Vector3 positionShakeOffset = new Vector3(perlinX, perlinY, 0f) * positionShakeIntensity;
            Vector3 rotationShakeOffset = new Vector3(0f, 0f, perlinRotZ) * rotationShakeIntensity;

            // Apply the shake offset to the camera position and rotation
            transform.position = _originalPosition + positionShakeOffset + _cameraOffset;
            transform.rotation = _originalRotation * Quaternion.Euler(rotationShakeOffset);

            // Increment the elapsed time
            _shakeTimer += Time.deltaTime;
        }
        else
        {
            // Reset the camera position and rotation after the shake duration
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            _shakeTimer = 0;
            _performShake = false;
        }
    }
    
}
