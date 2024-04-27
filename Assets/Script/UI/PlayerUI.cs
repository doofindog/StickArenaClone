using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("health Bar")] 
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private GameObject healthHolder;
    

    public void OnEnable()
    {
        PlayerEvents.PlayerSpawnedEvent += OnPlayerSpawned;
        PlayerEvents.DamageTakenEvent += UpdateHealth;
    }

    public void OnDisable()
    {
        PlayerEvents.PlayerSpawnedEvent -= OnPlayerSpawned;
        PlayerEvents.DamageTakenEvent -= UpdateHealth;
    }

    private void OnPlayerSpawned(GameObject playerObj)
    {
        UpdateHealth(playerObj.GetComponent<CharacterDataHandler>());
    }

    private void UpdateHealth(CharacterDataHandler playerData)
    {
        DisableAllHearts();
        
        for(int i = 1; i <= playerData.health.Value; i++)
        {
            if (i > healthHolder.transform.childCount)
            {
               AddHeart();
            }
            
            GameObject heart = healthHolder.transform.GetChild(i-1).gameObject;
            heart.gameObject.SetActive(true);
            heart.GetComponent<Animator>().Play("TakeDamage");
        }
    }

    private void AddHeart()
    {
        Instantiate(healthPrefab, healthHolder.transform);
    }

    private void DisableAllHearts()
    {
        foreach (Transform heart in healthHolder.transform)
        {
            heart.gameObject.SetActive(false);
        }
    }
}
