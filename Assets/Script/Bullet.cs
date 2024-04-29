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
    private NetworkVariable<ulong> _playerNetID = new NetworkVariable<ulong>();
    private Animator _animator;

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        TickManager.Instance.RemoveEntity(this);
    }

    public void Awake()
    {
        TickManager.Instance.AddEntity(this);
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
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateTick(int tick)
    {
        if(!_isEnabled) return;
        
        transform.position += transform.right * (_speed * TickManager.Instance.GetMinTickTime());
        _life += TickManager.Instance.GetMinTickTime();
        if (_life > BULLET_LIFE)
        {
            HandleImpact();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out NetworkObject networkObject))
        {
            bool canTakeDamage = networkObject.NetworkObjectId != _playerNetID.Value;
            if (canTakeDamage)
            {
                if (other.TryGetComponent(out HitDetector hitDetector))
                {
                    HitResponseData responseData = new HitResponseData()
                    {
                        hitTime = NetworkManager.Singleton.ServerTime.Time,
                        damage = _damage,
                        sourceID = _playerNetID.Value,
                        hitId = NetworkManager.LocalClient.ClientId,
                        hitPosition =  other.transform.position
                    };
                    
                    hitDetector.Hit(responseData);
                }
            }
        }

        GameObject particle = ObjectPool.Instance.GetPooledObject(impactParticle, transform.position, quaternion.identity).gameObject;
        particle.SetActive(true);
        HandleImpact();
    }

    public void HandleImpact()
    {
        _isEnabled = false;
        if (IsServer)
        {
            CoroutineHelper.Instance.StartCoroutine(ServerDestroy());
        }
        else
        {
            gameObject.SetActive(false);
            transform.position = new Vector3(-1000, -1000, -1000);
        }
    }

    private IEnumerator ServerDestroy()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(1);
        if (IsSpawned)
        {
            GetComponent<NetworkObject>().Despawn(true);
        }

        transform.position = new Vector3(-1000, -1000, -1000);
    }
}