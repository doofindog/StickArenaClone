using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private PreGameUI prePanel;
    [SerializeField] private DeathScreenUI deathPanel;
    [SerializeField] private GameoverUI gameoverUI;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
        GameEvents.GameOverEvent += DisplayGameOverScreen;
    }

    public void OnEnable()
    {
        DisplayPreGameScreen();
    }

    private void DisplayDeathScreen()
    {
        deathPanel.gameObject.SetActive(true);
    }

    private void DisplayPreGameScreen()
    {
        gameObject.SetActive(true);
        prePanel.gameObject.SetActive(true);
    }

    private void DisplayGameOverScreen(TeamType teamType)
    {
        gameoverUI.gameObject.SetActive(true);
        gameoverUI.SetText(teamType);
    }
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
