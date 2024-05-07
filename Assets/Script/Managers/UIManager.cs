using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum Screens
{
    Menu = 0,
    Game = 1,
    GameOver = 2,
    None = 3,
}

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject[] screens;

    protected override void Awake()
    {
        base.Awake();
        
        GameEvents.OnGameStateChange += OnGameStateChange;
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
                _canvas = GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                break;
            }
        }
    }
}
