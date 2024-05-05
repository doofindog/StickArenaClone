using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour , IPickable
{
    [SerializeField] protected GameObject pickUpParticle;

    private float _timer;
    private bool canCollect;
    private Vector2 _endPosition;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _itemCollider;
    [SerializeField] private Rigidbody2D _rb;
    private Action _onItemCollected;
    
    public void Initialise(Vector2 initialVelocity,Vector2 endPosition, Action onTimeCollected)
    {
        _endPosition = endPosition;
        _onItemCollected = onTimeCollected;
        
        _rb = GetComponent<Rigidbody2D>();
        _rb.AddForce(initialVelocity, ForceMode2D.Impulse);
    }

    public void Start()
    {
        _itemCollider = GetComponent<Collider2D>();
        _itemCollider.enabled = false;
        
        _spriteRenderer = transform.GetComponentInChildren<SpriteRenderer>(true);
        _spriteRenderer.color = new Color(1f, 1f, 1f, 0.11f);
        
        _rb = GetComponent<Rigidbody2D>();
    }
    
    public void Update()
    {
        _timer += Time.deltaTime; 
        if (_timer > 0.5f && _spriteRenderer.enabled == false)
        {
            _spriteRenderer.enabled = true;
            _itemCollider.enabled = true;
        }

        if (IsServer && !canCollect)
        {
            if (Vector3.Distance(transform.position, _endPosition) < 1f)
            {
                canCollect = true;
                _rb.velocity = Vector3.zero;
                _rb.bodyType = RigidbodyType2D.Kinematic;
                _itemCollider.enabled = true;
                _spriteRenderer.color = Color.white;
                
                SendCanCollectClientRPC();
            }
        }
    }

    [ClientRpc]
    private void SendCanCollectClientRPC()
    {
        _itemCollider.enabled = true;
        _spriteRenderer.color = Color.white;
    }

    public void OnPickUp(ulong clientID)
    {
        SpawnManager.Instance.SpawnObject
        (
            prefab: pickUpParticle,
            spawnType: SpawnManager.SpawnType.MONO,
            position: transform.position,
            rotation: transform.rotation
        );
        
        OnPickUpServerRPC(clientID);

        _spriteRenderer.enabled = false;
        _itemCollider.enabled = false;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void OnPickUpServerRPC(ulong clientID)
    {
        Effect[] effects = GetComponents<Effect>();
        foreach (Effect effect in effects)
        {
            effect.AddEffectToPlayer(clientID);
        }
        
        NetworkObject.Despawn(this);
        _onItemCollected?.Invoke();
    }
}
