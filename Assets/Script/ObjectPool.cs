using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private List<PoolObjConfig> pools;

    private Dictionary<GameObject, List<GameObject>> _objectPoolDictionary = new Dictionary<GameObject, List<GameObject>>();

    protected override void Awake()
    {
        GameEvents.OnGameStartEvent += InitializeObjectPools;
        GameEvents.OnGameOverEvent += ClearPool;
    }

    void InitializeObjectPools()
    {
        _objectPoolDictionary = new Dictionary<GameObject, List<GameObject>>();

        foreach (PoolObjConfig pool in pools)
        {
            List<GameObject> objectPool = new List<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Add(obj);
            }

            _objectPoolDictionary.Add(pool.prefab, objectPool);
        }
    }

    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (_objectPoolDictionary.TryGetValue(prefab, out List<GameObject> objectPool))
        {
            foreach (var pooledObjects in objectPool)
            {
                if (!pooledObjects.activeInHierarchy)
                {
                    pooledObjects.transform.position = position;
                    pooledObjects.transform.rotation = rotation;
                    pooledObjects.SetActive(true);
                    return pooledObjects;
                }
            }

            // If no inactive object is found, create a new one (expand the pool if needed)
            GameObject newObj = Instantiate(prefab, position, rotation);
            newObj.SetActive(true);
            objectPool.Add(newObj);
            return newObj;
        }

        Debug.LogWarning($"Prefab {prefab.name} is not in the object pool dictionary. Make sure to add it to the 'pools' list in the Inspector.");
        return null;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void ClearPool()
    {
        foreach (GameObject key in _objectPoolDictionary.Keys) 
        {
            foreach (GameObject obj in _objectPoolDictionary[key])
            {
                Destroy(obj.gameObject);
            }
        }
        
        _objectPoolDictionary.Clear();
    }
}

[System.Serializable]
public class PoolObjConfig
{
    public GameObject prefab;
    public int size;
}
