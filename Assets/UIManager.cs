using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startScreenPanel;
    [SerializeField] private GameObject gameUIPanel;

    public void Awake()
    {
        GameEvents.StartGameEvent += HandleOnGameStarted;
    }

    private void HandleOnGameStarted()
    {
        startScreenPanel.SetActive(false);
        gameUIPanel.SetActive(true);
    }

    private void HandleOnGameEnded()
    {
        
    }
}
