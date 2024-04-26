using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public interface IPickable
{
    public void OnPickUp(ulong clientID);

    [ServerRpc]
    public void OnPickUpServerRPC(ulong clientID);
}
