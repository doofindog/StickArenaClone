using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private Button respawnButton;
    
    public void Awake()
    {
        respawnButton.onClick.AddListener(SpawnPlayer);
    }

    public void OnEnable()
    {
        respawnButton.gameObject.SetActive(false);
        counterText.gameObject.SetActive(true);
        StartCoroutine(StartRespawnCounter());
    }

    private IEnumerator StartRespawnCounter()
    {
        SessionSettings sessionSettings = GameManager.Instance.GetSessionSettings();
        float counter = sessionSettings.playerRespawnTime;
        counterText.text = counter.ToString(CultureInfo.InvariantCulture);
        
        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
            counterText.text = counter.ToString(CultureInfo.InvariantCulture);
        }
        
        counterText.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(true);
    }

    private void SpawnPlayer()
    {
        respawnButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
        GameManager.Instance.RequestSpawnPlayerServerRPC(Unity.Netcode.NetworkManager.Singleton.LocalClientId);
    }
}
