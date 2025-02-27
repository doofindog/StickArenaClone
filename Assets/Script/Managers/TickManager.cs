using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class TickManager : NetworkBehaviour
{
    public static TickManager Instance;

    [SerializeField] private float serverTickRate;

    private bool _enable;
    [SerializeField] private int m_tick;
    [SerializeField] private float m_timer;
    [SerializeField] private float m_minTimeBetweenTicks; //how many seconds between each tick
    [SerializeField] private float m_lastTickTime;
    [SerializeField] private float m_tickDeltaTime;
    private List<ITickableEntity> _tickableEntities = new List<ITickableEntity>();

    private void Awake()
    {
        Instance = this;
    }


    public void Init()
    {
        NetworkManager.Singleton.OnClientStarted += Init;
        NetworkManager.Singleton.OnServerStarted += Init;

        NetworkManager.Singleton.OnClientStopped += OnNetworkStopped;
        NetworkManager.Singleton.OnServerStopped += OnNetworkStopped;

        m_lastTickTime = Time.time;
        m_minTimeBetweenTicks = 1f / serverTickRate;
        _enable = m_minTimeBetweenTicks != 0;
    }

    private void OnNetworkStopped(bool obj)
    {
        NetworkManager.Singleton.OnClientStarted -= Init;
        NetworkManager.Singleton.OnServerStarted -= Init;
    }

    public void AddEntity(ITickableEntity tickableEntity)
    {
        _tickableEntities.Add(tickableEntity);
    }

    public void RemoveEntity(ITickableEntity tickableEntity)
    {
        _tickableEntities.Remove(tickableEntity);
    }

    public void Update()
    {
        if (!_enable)
        {
            return;
        }

        m_timer += Time.deltaTime;
        while (m_timer >= m_minTimeBetweenTicks)
        {
            m_timer -= m_minTimeBetweenTicks;
            if (m_timer < m_minTimeBetweenTicks)
            {
                m_lastTickTime += m_minTimeBetweenTicks;
            }

            foreach (ITickableEntity entity in _tickableEntities.ToList())
            {
                entity.UpdateTick(m_tick);
            }

            m_tick++;
        }
    }

    public int GetTick()
    {
        return m_tick;
    }
     
    public float GetMinTickTime()
    {
        return m_minTimeBetweenTicks;
    }

    public float GetTickRate()
    {
        return serverTickRate;
    }

    public float GetTickDeltaTime()
    {
        return m_lastTickTime;
    }
}
