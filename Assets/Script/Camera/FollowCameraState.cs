using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FollowCameraState : CameraState
{
    public Camera uiCamera;
    
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _interpolationSpeed;
    
    [Header("Shake")] 
    [SerializeField] private float _positionShakeIntensity;
    [SerializeField] private float _rotationShakeIntensity;
    [SerializeField] private float _shakeDuration;
    [SerializeField] private float _shakeTimer;
    private bool _performShake;
    [SerializeField] private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    [SerializeField] private PixelPerfectCamera _pixelPerfectCamera;

    public void Awake()
    {
        LocalPlayerEvents.LocalPlayerSpawnedEvent += SetTarget;
    }


    public override void Enter()
    {
        LocalPlayerEvents.PlayerDiedEvent += PerformShake;
        GameEvents.WeaponFiredEvent += PerformShake;
        
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
        
        UniversalAdditionalCameraData cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;
        _pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        _pixelPerfectCamera.gridSnapping = PixelPerfectCamera.GridSnapping.None;
    }

    public void SetTarget(GameObject pTarget)
    {
        _follow = pTarget.transform;
    }

    public override void UpdateState()
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

    public override void Exit()
    {
        LocalPlayerEvents.PlayerDiedEvent -= PerformShake;
        GameEvents.WeaponFiredEvent -= PerformShake;
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

    public void Update()
    {
        if (_follow == null) return;

        Vector3 followPosition = _follow.position + _cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, followPosition,
            _interpolationSpeed * TickManager.Instance.GetMinTickTime());

        _originalPosition = smoothedPosition;
        transform.position = smoothedPosition;
    }
}
