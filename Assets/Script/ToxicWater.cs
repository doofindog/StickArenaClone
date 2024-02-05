using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ToxicWater : MonoBehaviour
{
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (other.CompareTag("Player"))
            {
                NetController[] components = other.GetComponents<NetController>();
                foreach (NetController component in components)
                {
                    component.Drown();
                }
            }
        }
    }
}
