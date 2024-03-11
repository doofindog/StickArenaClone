using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private PreGameUI prePanel;
    [SerializeField] private DeathPanel deathPanel;
    [SerializeField] private GameoverScreen gameoverScreen;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
        GameEvents.GameOverEvent += DisplayGameOverScreen;
        GameEvents.PreparingArenaEvent += DisplayPreGameScreen;
    }

    private void DisplayDeathScreen()
    {
        deathPanel.gameObject.SetActive(true);
    }

    private void DisplayPreGameScreen()
    {
        Debug.Log("Called");
        prePanel.gameObject.SetActive(true);
    }

    private void DisplayGameOverScreen(TeamType teamType)
    {
        gameoverScreen.gameObject.SetActive(true);
        gameoverScreen.SetText(teamType);
    }
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
