using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private PreGameUI prePanel;
    [SerializeField] private DeathScreenUI deathPanel;
    
    public void Awake()
    {
        PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
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
    
    public void OnDestroy()
    {
        PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
    }
}
