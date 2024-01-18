using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_LIFE = 3.0f;
    
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
        TickManager.Instance.AddEntity(this);
        _isEnabled = true;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        TickManager.Instance.RemoveEntity(this);
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
            IDamageableEntity damageableEntity = null;
            bool canTakeDamage = networkObject.NetworkObjectId != _playerNetID.Value
                                 && other.gameObject.TryGetComponent(out damageableEntity);
            if (IsServer)
            {
                Debug.Log("Can take Damage" + canTakeDamage);
            }
            if (canTakeDamage)
            { 
                damageableEntity.TakeDamage(_damage);
            }
        }
     
        _isEnabled = false;
        _animator.Play("Impact");
    }

    public void HandleImpact()
    {
        gameObject.SetActive(false);
        transform.position = new Vector3(-1000, -1000, -1000);
    }
}