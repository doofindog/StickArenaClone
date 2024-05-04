using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour , IPickable
{
    [SerializeField] protected GameObject pickUpParticle;
    [SerializeField] protected bool autoSpawn;
    
    protected SpriteRenderer spriteRenderer;
    protected Collider2D itemCollider;
    protected float timer;
    
    public void Start()
    {
        spriteRenderer = transform.GetComponentInChildren<SpriteRenderer>(true);
        itemCollider = GetComponent<Collider2D>();
        
        if (IsClient && !IsHost)
        {
            Destroy(GetComponent<Collider2D>());
        }
    }
    
    public void Update()
    {
        timer += Time.deltaTime; 
        if (timer > 0.5f && spriteRenderer.enabled == false)
        {
            spriteRenderer.enabled = true;
            itemCollider.enabled = true;
        }
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

        spriteRenderer.enabled = false;
        itemCollider.enabled = false;
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
    }
}
