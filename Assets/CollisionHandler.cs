using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class CollisionHandler : NetworkBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (IsOwner && (IsClient || IsHost))
        {
            if (other.CompareTag($"Item"))
            {
                IPickable[] components = other.GetComponents<IPickable>();
                foreach (IPickable component in components)
                {
                    component.OnPickUp(GetComponent<NetworkObject>().OwnerClientId);
                }
            }
        }
    }
}
