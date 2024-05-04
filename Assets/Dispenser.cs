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

    private void DispenseItem()
    {
        if (!(timer > dispenseRate && dispenseCount >= maxDispenseCount))
        {
            return;
        }
        
        foreach (DispenserItemData itemData in dispenserItems)
        {
            if(Random.Range(0,1) > itemData.priority) return; 
            
            GameObject itemObj = SpawnManager.Instance.SpawnObject(itemData.prefab, SpawnManager.SpawnType.NETWORK, transform.position, Quaternion.identity);
            NetworkObject itemNetObj = itemObj.GetComponent<NetworkObject>();
            itemNetObj.Spawn();
            dispenseCount++;
                
            _dispensedItems.Add(new DispensedItem());
        }

        timer += Time.deltaTime;
    }
}
