using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_SPEED = 15.0f;
    private const float BULLET_LIFE = 3.0f;

    
    private bool _isEnabled;
    private bool _hasHitObstacle;
    private float _speed;
    private float _life;
    private NetworkVariable<ulong> _playerNetID = new NetworkVariable<ulong>();

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

    public void Initialise(ulong playerID)
    {
        _playerNetID.Value = playerID;
    }

    public void UpdateTick(int tick)
    {
        if (!_isEnabled) return;
        transform.position += transform.right * BULLET_SPEED * TickManager.Instance.GetMinTickTime();
        
        _life += TickManager.Instance.GetMinTickTime();
        if (_life > BULLET_LIFE)
        {
            HandleImpact();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.gameObject.TryGetComponent(out NetworkObject networkObject))
            {
                IDamageableEntity damageableEntity = null;
                bool canTakeDamage = networkObject.NetworkObjectId != _playerNetID.Value
                                     && other.gameObject.TryGetComponent(out damageableEntity);
                if (canTakeDamage)
                { 
                    damageableEntity.TakeDamage();
                }
            }
        }
        
        HandleImpact();
    }

    private void HandleImpact()
    {
        _isEnabled = false;
        gameObject.SetActive(false);
        transform.position = new Vector3(-1000, -1000, -1000);
    }
}
