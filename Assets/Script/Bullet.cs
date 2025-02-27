using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_LIFE = 3.0f;

    [SerializeField] private GameObject impactParticle;

    private int _id;
    private bool _isEnabled;
    private bool _hasHitObstacle;
    private int _damage;
    private float _life;
    private float _speed;
    private Vector3 _startPosition;
    private NetworkVariable<ulong> _playerNetID = new NetworkVariable<ulong>();
    private Animator _animator;

    public override void OnNetworkDespawn()
    {
        void HandleServer()
        {
            TickManager.Instance.RemoveEntity(this);
        }

        void HandleClient() { }

        base.OnNetworkDespawn();
        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    public void Awake()
    {
        void HandleServer()
        {
            TickManager.Instance.AddEntity(this);
        }

        void HandleClient() { }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    public void OnEnable()
    {
        _isEnabled = true;
        _life = 0;
    }

    public void OnDisable()
    {
        _isEnabled = false;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        
        TickManager.Instance.RemoveEntity(this);
    }

    public void Initialise(ulong playerID, int damage, float bulletSpeed)
    {
        _playerNetID.Value = playerID;
        _speed = bulletSpeed;
        _damage = damage;
        _startPosition = transform.position;
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void FixedUpdate()
    {
        transform.position += transform.right * (_speed * Time.fixedDeltaTime);
        _life += Time.fixedDeltaTime;

        if (_life > BULLET_LIFE)
        {
            HandleImpact();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        //if (other.gameObject.TryGetComponent(out NetworkObject networkObject))
        //{
        //    bool canTakeDamage = networkObject.NetworkObjectId != _playerNetID.Value;
        //    if (canTakeDamage)
        //    {
        //        if (other.TryGetComponent(out HitDetector hitDetector))
        //        {
        //            HitResponseData responseData = new HitResponseData()
        //            {
        //                hitTime = NetworkManager.Singleton.ServerTime.TimeAsFloat,
        //                damage = _damage,
        //                sourceID = _playerNetID.Value,
        //                hitId = NetworkManager.LocalClient.ClientId,
        //                hitPosition =  other.transform.position,
        //                traceStart = _startPosition,
        //                projectileRotation = transform.rotation,
        //                hitVelocity = _speed * TickManager.Instance.GetMinTickTime(),
        //                projectileDirection = (transform.position + transform.right) - transform.position
        //            };
                    

        //            hitDetector.Hit(responseData);
        //        }
        //    }
        //}

        HandleImpact();
    }

    private void HandleImpact()
    {
        GameObject particle = ObjectPool.Instance.GetPooledObject(impactParticle, transform.position, quaternion.identity).gameObject;
        particle.SetActive(true);

        gameObject.SetActive(false);
        transform.position = new Vector3(-1000, -1000, -1000);
    }

    public void UpdateTick(int tick)
    {
        //throw new NotImplementedException();
    }
}