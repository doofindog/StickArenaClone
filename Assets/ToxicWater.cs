using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ToxicWater : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Called");
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.CompareTag("Player"))
            {
                ulong clientID = other.GetComponent<NetworkObject>().OwnerClientId;
                GameSessionManager.Singleton.DespawnPlayer(clientID);      
            }
        }
    }
}
