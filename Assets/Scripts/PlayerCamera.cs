using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCamera : MonoBehaviour, ITickableEntity
{
    [SerializeField] private Transform _follow;
    [SerializeField] private Vector3 _cameraOffset;
    [SerializeField] private float _interpolationSpeed;

    public void Awake()
    {
        GameEvents.PlayerConnectedEvent += HandlePlayerConnected;

    }

    private void HandlePlayerConnected(GameObject obj)
    {
        _follow = obj.transform;
        TickManager.Instance.AddEntity(this);
    }
    public void UpdateTick(int tick)
    {
        if(_follow == null) return;

        Vector3 followPosition = _follow.position + _cameraOffset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, followPosition,
            _interpolationSpeed * TickManager.Instance.GetMinTickTime());

        transform.position = smoothedPosition;

        /*float pixelsPerUnit = 32f;
        Vector3 pixelPosition = new Vector3(
            Mathf.Round(smoothedPosition.x * pixelsPerUnit) / pixelsPerUnit,
            Mathf.Round(smoothedPosition.y * pixelsPerUnit) / pixelsPerUnit,
            smoothedPosition.z);

        transform.position = Vector3.Lerp(smoothedPosition, pixelPosition, _interpolationSpeed);*/
    }
}
