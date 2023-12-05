using System;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour, ITickableEntity
{
    private const float BULLET_SPEED = 10.0f;

    private NetworkVariable<ulong> _playerNetID = new NetworkVariable<ulong>();
    private NetworkVariable<Vector3> netPosition = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        TickManager.Instance.AddEntity(this);
        if (IsServer)
        {
            netPosition.Value = transform.position;
        }
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
        if (IsServer)
        {
            ProcessMovementServer();
        }

        if (IsClient)
        {
            ProcessMovementClient();
        }
    }

    private void ProcessMovementServer()
    {
        transform.position += transform.up * BULLET_SPEED * TickManager.Instance.GetMinTickTime();
        netPosition.Value = transform.position;
    }

    private void ProcessMovementClient()
    {
        transform.position = Vector2.Lerp(transform.position, netPosition.Value, 0.25f);
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
            
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
