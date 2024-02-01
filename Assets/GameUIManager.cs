using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private StartGameUI startPanel;
    [SerializeField] private DeathScreenUI deathPanel;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
        GameEvents.StartGameEvent += DisplayStarGameScreen;
    }

    private void DisplayDeathScreen()
    {
        deathPanel.gameObject.SetActive(true);
    }

    private void DisplayStarGameScreen()
    {
        startPanel.gameObject.SetActive(true);
    }
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
