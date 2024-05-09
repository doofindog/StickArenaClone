using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public enum Screens
{
    Menu = 0,
    Game = 1,
    GameOver = 2,
    None = 3,
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Canvas _gameCanvas;
    [SerializeField] private Canvas _tvCanvas;
    [SerializeField] private Camera uiCamera;
    [SerializeField] private GameObject[] screens;

    protected override void Awake()
    {
        base.Awake();
        
        GameEvents.OnGameStateChange += OnGameStateChange;
        GameEvents.TeamWonEvent += TeamWonEvent;
        _gameCanvas = GetComponent<Canvas>();
    }

    private void TeamWonEvent(TeamType obj)
    {
        _gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        if (uiCamera != null)
        {
            _gameCanvas.worldCamera = uiCamera;
        }
    }

    public GameObject ReplaceScreen(Screens screenEnum)
    {
        foreach (GameObject screen in screens)
        {
            screen.SetActive(false);
        }

        if (screens is { Length: > 0 } && (int)screenEnum < screens.Length)
        {
            screens[(int)screenEnum].SetActive(true);
            return screens[(int)screenEnum];
        }

        return null;
    }

    public T GetScreen<T>(Screens screenEnum)
    {
        return screens[(int)screenEnum].GetComponent<T>();
    }

    private void OnGameStateChange(EGameStates state)
    {
        switch (state)
        {
            case EGameStates.GAME:
            {
                _gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                break;
            }
            case EGameStates.MENU:
            {
                _gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                if (uiCamera != null)
                {
                    _gameCanvas.worldCamera = uiCamera;
                }
                break;
            }
        }
    }
}
