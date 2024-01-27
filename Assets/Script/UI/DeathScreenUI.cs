using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Button respawnButton;
    
    public void Awake()
    {
        respawnButton.onClick.AddListener(SpawnPlayer);
    }

    public void ShowScreen()
    {
        gameObject.SetActive(true);
        counterText.gameObject.SetActive(true);
        StartCoroutine(StartRespawnCounter());
    }

    private IEnumerator StartRespawnCounter()
    {
        SessionSettings sessionSettings = GameSessionManager.Singleton.GetSessionSettings();
        int counter = sessionSettings.playerRespawnTime;
        counterText.text = counter.ToString();
        
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            counterText.text = counter.ToString();
        }
        
        counterText.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(true);
    }

    private void SpawnPlayer()
    {
        respawnButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
        GameSessionManager.Singleton.RequestSpawnPlayerServerRPC(Unity.Netcode.NetworkManager.Singleton.LocalClientId);
    }
}
