using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public enum SpawnType
    {
        MONO,
        NETWORK,
    }

    [SerializeField] private ObjectPool _monoObjectPool;
    [SerializeField] private NetworkObjectPool _networkObjectPool;
    

    public GameObject SpawnObject(GameObject prefab, SpawnType spawnType, Vector3 position, Quaternion rotation)
    {
        switch (spawnType)
        {
            case SpawnType.MONO:
                return _monoObjectPool.GetPooledObject(prefab, position, rotation);
            case SpawnType.NETWORK:
                return _networkObjectPool.GetNetworkObject(prefab, position, rotation).gameObject;
            default:
                return null;
        }
    }
}
