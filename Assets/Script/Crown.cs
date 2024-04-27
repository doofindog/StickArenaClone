using System;
using Unity.Netcode;
using UnityEngine;

public class Crown : NetworkBehaviour, ITickableEntity, IPickable
{
    [SerializeField] private float _followSpeed;
    [SerializeField] private GameObject _pickUpParticle;
    private float timer;
    private NetworkVariable<bool> _isAcquired = new NetworkVariable<bool>();
    private Transform _playerCrownHolder;
    

    public void Start()
    {
        _isAcquired.Value = false;

        if (IsClient && !IsHost)
        {
            Destroy(GetComponent<Collider2D>());
        }
        
        TickManager.Instance.AddEntity(this);
    }


    public override void OnDestroy()
    {
        TickManager.Instance.RemoveEntity(this);
    }

    public void Update()
    {
        timer += Time.deltaTime; 
        if (timer > 0.5f && GetComponent<SpriteRenderer>().enabled == false)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    [ClientRpc]
    private void PlacedCrownClientRpc(ulong clientID)
    {
        ConnectionManager manager = GameManager.Instance.connectionManager;
        NetworkObject playerNetObj = manager.GetPlayerNetObject(clientID);
        NetController netController = playerNetObj.GetComponent<NetController>();
        _playerCrownHolder = netController.GetCrownPlaceholder();
        netController.AddCrown(this);
        gameObject.GetComponent<Collider2D>().enabled = false;
    }

    public void UpdateTick(int tick)
    {
        if (_isAcquired.Value && _playerCrownHolder != null)
        {
            transform.position = Vector3.Lerp(transform.position, _playerCrownHolder.position, _followSpeed);
        }
    }

    public void OnPickUp(ulong clientID)
    {
        SpawnManager.Instance.SpawnObject
        (
            prefab: _pickUpParticle,
            spawnType: SpawnManager.SpawnType.MONO,
            position: transform.position,
            rotation: transform.rotation
        );
        
        OnPickUpServerRPC(clientID);
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnPickUpServerRPC(ulong clientID)
    {
        ScoreManager.Instance.AddScore(clientID);
        NetworkObject.Despawn(false);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
        base.OnNetworkDespawn();
    }

    public override void OnNetworkSpawn()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }
}
