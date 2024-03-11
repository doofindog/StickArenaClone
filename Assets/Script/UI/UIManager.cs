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
    [SerializeField] private GameObject[] screens;

    public void ReplaceScreen(Screens screenEnum)
    {
        foreach (GameObject screen in screens)
        {
            screen.SetActive(false);
        }

        if (screens != null && screens.Length > 0 && (int)screenEnum < screens.Length)
        {
            screens[(int)screenEnum].SetActive(true);
        }
    }
}
