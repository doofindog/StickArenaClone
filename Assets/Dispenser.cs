using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class DispenserItemData
{
    public GameObject prefab;
    public float priority;
}

public class DispensedItem
{
    public Vector3 initialPosition;
    public Vector3 landingPosition;
    public GameObject item;
}

public class Dispenser : NetworkBehaviour
{
    public DispenserItemData[] dispenserItems;
    public float dispenseCount;
    public float maxDispenseCount;
    public float dispenseRate;
    public float timer;

    private List<DispensedItem> _dispensedItems = new List<DispensedItem>();


    public void Update()
    {
        if (IsServer && GameManager.Instance.GetState() == EGameStates.GAME)
        {
            DispenseItem();
        }
    }

    private void DispenseItem()
    {
        if (timer <= dispenseRate || dispenseCount >= maxDispenseCount)
        {
            timer += Time.deltaTime;
            return;
        }
        
        foreach (DispenserItemData itemData in dispenserItems)
        {
            if(Random.Range(0.0f,1.0f) > itemData.priority) continue; 
            
            GameObject itemObj = SpawnManager.Instance.SpawnObject(itemData.prefab, SpawnManager.SpawnType.NETWORK, transform.position, Quaternion.identity);
            NetworkObject itemNetObj = itemObj.GetComponent<NetworkObject>();
            float time = 1.0f;
            Vector2 endPosition = GetPointsInPlain() + new Vector2(0.5f, 0.5f);
            Vector2 initialVelocity = CalculateInitialForce(endPosition, time);
            _dispensedItems.Add(new DispensedItem());
            itemNetObj.Spawn();
            itemObj.GetComponent<Item>().Initialise(initialVelocity, endPosition, ItemCollected);
            
            dispenseCount++;
            
            break;
        }

        timer = 0;
    }

    private Vector2 CalculateInitialForce(Vector2 endPosition, float time)
    {
        Vector2 gravity = new Vector2(Physics2D.gravity.x, Mathf.Abs(Physics2D.gravity.y));
        return ((endPosition - Vector2.zero) / time) +  (0.5f * time * gravity);
    }

    private Vector2 GetPointsInPlain()
    {
        bool chooseLeftSide = Random.value > 0.5f;
        float x = chooseLeftSide ? Random.Range(-4, 0) : Random.Range(0, 4);

        bool chooseBottomSide = Random.value > 0.5f;
        float y = chooseBottomSide ? Random.Range(-4, 0) : Random.Range(0, 4);

        return new Vector2(x, y);
    }

    private void ItemCollected()
    {
        dispenseCount--;
    }
}
