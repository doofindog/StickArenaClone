using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTimeTest : MonoBehaviour, ITickableEntity
{
    [SerializeField] private bool enabled;
    [SerializeField] private double serverTime;
    [SerializeField] private double localTime;
    
    private void Start()
    {
        TickManager.Instance.AddEntity(this);
    }

    public void UpdateTick(int tick)
    {
        if(!enabled) return;

        serverTime = NetworkManager.Singleton.ServerTime.Time;
        localTime = NetworkManager.Singleton.LocalTime.Time;
        
        Debug.Log("[TIME] Network Time : " + serverTime + ", Local Time :" + localTime);
    }
}
