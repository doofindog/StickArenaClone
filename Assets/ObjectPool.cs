using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private List<PoolObjConfig> pools;

    private Dictionary<GameObject, List<GameObject>> _objectPoolDictionary = new Dictionary<GameObject, List<GameObject>>();
    
    
    void Start()
    {
        InitializeObjectPools();
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
            for (int i = 0; i < objectPool.Count; i++)
            {
                if (!objectPool[i].activeInHierarchy)
                {
                    objectPool[i].transform.position = position;
                    objectPool[i].transform.rotation = rotation;
                    return objectPool[i];
                }
            }

            // If no inactive object is found, create a new one (expand the pool if needed)
            GameObject newObj = Instantiate(prefab, position, rotation);
            newObj.SetActive(false);
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
}

[System.Serializable]
public class PoolObjConfig
{
    public GameObject prefab;
    public int size;
}
