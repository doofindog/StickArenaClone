using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class TickManager : Singleton<TickManager>
{
    [SerializeField] private float serverTickRate;

    private bool _enable;
    private int _tick;
    private float _timer;
    private float _minTimeBetweenTicks; //how many seconds between each tick
    private List<ITickableEntity> _tickableEntities = new List<ITickableEntity>();
    
    
    public void Init()
    {
        NetworkManager.Singleton.OnClientStarted += Init;
        NetworkManager.Singleton.OnServerStarted += Init;
        
        _minTimeBetweenTicks = 1f / serverTickRate;
        _enable = _minTimeBetweenTicks != 0;
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
        
        _timer += Time.deltaTime;
        while (_timer >= _minTimeBetweenTicks)
        {
            _timer -= _minTimeBetweenTicks;
            foreach (ITickableEntity entity in _tickableEntities.ToList())
            {
                entity.UpdateTick(_tick);
            }

            _tick++;
        }
    }

    public int GetTick()
    {
        return _tick;
    }

    public float GetMinTickTime()
    {
        return _minTimeBetweenTicks;
    }

    public float GetTickRate()
    {
        return serverTickRate;
    }
}
