using System;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_LIFE = 3.0f;

    [SerializeField] private GameObject impactParticle;
    
    private bool _isEnabled;
    private bool _hasHitObstacle;
    private float _damage;
    private float _life;
    private NetworkVariable<float> _speed = new NetworkVariable<float>();
    private NetworkVariable<ulong> _playerNetID = new NetworkVariable<ulong>();
    private Animator _animator;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        gameObject.SetActive(true);
        TickManager.Instance.AddEntity(this);
        _isEnabled = true;
        _life = 0;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        TickManager.Instance.RemoveEntity(this);
        _isEnabled = false;
    }

    public void Initialise(ulong playerID, float damage, float bulletSpeed)
    {
        _playerNetID.Value = playerID;
        if (IsServer)
        {
            _speed.Value = bulletSpeed;
        }
        _damage = damage;
    }

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateTick(int tick)
    {
        if(!_isEnabled) return;
        
        transform.position += transform.right * (_speed.Value * TickManager.Instance.GetMinTickTime());
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
                NetworkObject source = GameManager.Singleton.GetPlayerNetObject(_playerNetID.Value);
                IDamageableEntity[] components = other.GetComponents<IDamageableEntity>();
                foreach (IDamageableEntity component in components)
                {
                    component.TakeDamage(_damage, source);
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