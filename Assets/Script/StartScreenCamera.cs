using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;

public class StartScreenCamera : MonoBehaviour
{
    [SerializeField] private float _cameraSpeed;
    [SerializeField] private Transform[] _locations;

    private Transform _followTransform;
    private Vector3 _direction;

    private void Start()
    {
        UpdateNextLocation();
    }

    private void Update()
    {
        //throw new NotImplementedException();
        if (Vector2.Distance(transform.position, _followTransform.position) <= 0.1f)
        {
            UpdateNextLocation();
        }
        else
        {
            //transform.position= Vector3.Lerp(transform.position, _followTransform.position, 0.4f * Time.deltaTime );
            transform.position += _direction * Time.deltaTime * _cameraSpeed;
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
}
