using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private PreGameUI prePanel;
    [SerializeField] private DeathPanel deathPanel;
    [SerializeField] private GameoverScreen gameOverScreen;
    [SerializeField] private scoreUI scoreUI;
    [SerializeField] private GameObject playerHealth;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
        GameEvents.GameOverEvent += DisplayGameOverScreen;
        GameEvents.PreparingArenaEvent += DisplayPreGameScreen;
        GameEvents.StartGameEvent += DisplayGameUI;
    }

    private void DisplayGameUI()
    {
        playerHealth.SetActive(true);
        scoreUI.gameObject.SetActive(true);
    }

    private void DisplayDeathScreen()
    {
        deathPanel.gameObject.SetActive(true);
    }

    private void DisplayPreGameScreen()
    {
        prePanel.gameObject.SetActive(true);
    }

    private void DisplayGameOverScreen(TeamType teamType)
    {
        gameOverScreen.gameObject.SetActive(true);
        gameOverScreen.SetText(teamType);
    }
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
