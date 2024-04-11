using System;
using Unity.Netcode;
using UnityEngine;

public class Crown : NetworkBehaviour, ITickableEntity
{
    [SerializeField] private float _followSpeed;

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

    public void OnTriggerEnter2D(Collider2D other)
    {
        if(!IsServer) return;
        
        if (other.CompareTag("Player") && !_isAcquired.Value)
        {
            PlaceCrown(other.gameObject);
        }
    }

    private void PlaceCrown(GameObject player)
    {
        _isAcquired.Value = true;
        NetworkObject playerNetObj = player.GetComponent<NetworkObject>();
        
        PlacedCrownClientRpc(playerNetObj.OwnerClientId);
        
        GameEvents.SendCrownAcquired(playerNetObj.OwnerClientId);
    }

    [ClientRpc]
    private void PlacedCrownClientRpc(ulong clientID)
    {

        NetworkObject playerNetObj = GameManager.Instance.GetPlayerNetObject(clientID);
        _playerCrownHolder = playerNetObj.GetComponent<NetController>().GetCrownPlaceholder();
        gameObject.GetComponent<Collider2D>().enabled = false;
    }


    private void DropCrown(GameObject player)
    {
        _isAcquired.Value = false;
    }

    public void UpdateTick(int tick)
    {
        if (_isAcquired.Value && _playerCrownHolder != null)
        {
            transform.position = Vector3.Lerp(transform.position, _playerCrownHolder.position, _followSpeed);
        }
    }
}
