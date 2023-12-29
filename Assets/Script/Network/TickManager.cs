using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class TickManager : Singleton<TickManager>
{
    [SerializeField] private float serverTickRate;

    [SerializeField] private bool _enable;
    [SerializeField] private int _tick;
    [SerializeField] private float _timer;
    private float _minTimeBetweenTicks; //how many seconds between each tick
    private List<ITickableEntity> _tickableEntities = new List<ITickableEntity>();

    public void AddEntity(ITickableEntity tickableEntity)
    {
        Debug.Log("Added");
        _tickableEntities.Add(tickableEntity);
    }

    public void RemoveEntity(ITickableEntity tickableEntity)
    {
        _tickableEntities.Remove(tickableEntity);
    }
    
    public void Init()
    {
        if(_enable == true) return;
        
        _minTimeBetweenTicks = 1f / serverTickRate;
        _enable = _minTimeBetweenTicks != 0;
        Debug.Log("Min Time Between Ticks" + _minTimeBetweenTicks);
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
            foreach (ITickableEntity entity in _tickableEntities)
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
    
}
