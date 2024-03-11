using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameStates
{
    MENU,
    GAME,
    OVER
}


public class GameStateController : Singleton<GameStateController>
{
    private BaseGameState currentState;
    private Dictionary<GameStates, BaseGameState> gameStates = new Dictionary<GameStates, BaseGameState>();

    protected override void Awake()
    {
        base.Awake();
        
        gameStates.Add(GameStates.MENU, GetComponent<MenuState>());
        gameStates.Add(GameStates.GAME, GetComponent<GameState>());
    }

    public void SwitchState(GameStates state)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState = GetGameState(state);
        currentState.OnEnter();
    }

    private BaseGameState GetGameState(GameStates state)
    {
        gameStates.TryGetValue(state, out BaseGameState gameState);
        return gameState;
    }
}
