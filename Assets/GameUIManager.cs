using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private DeathScreenUI deathPanel;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
    }

    private void DisplayDeathScreen()
    {
        deathPanel.gameObject.SetActive(true);
        deathPanel.ShowScreen();
    }
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
