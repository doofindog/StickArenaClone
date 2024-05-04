using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HitDetector : NetworkBehaviour
{
    public void Hit(HitResponseData hitResponseData)
    {
        if(!IsClient) return;
        
        if (TryGetComponent(out ClientController clientController))
        {
            clientController.TakeDamage(hitResponseData);
        }

        if (NetworkManager.LocalClient.ClientId == hitResponseData.sourceID)
        {
            SendHitDetectedServerRpc(hitResponseData);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SendHitDetectedServerRpc(HitResponseData responseData)
    {
        GetComponent<ServerController>().TakeDamage(responseData);
    }
}
