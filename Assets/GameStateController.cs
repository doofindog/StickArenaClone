using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EGameStates
{
    MENU,
    GAME,
    OVER
}


public class GameStateController : Singleton<GameStateController>
{
    private BaseGameState currentState;
    private Dictionary<EGameStates, BaseGameState> gameStates = new Dictionary<EGameStates, BaseGameState>();

    protected override void Awake()
    {
        base.Awake();
        
        gameStates.Add(EGameStates.MENU, GetComponent<MenuState>());
        gameStates.Add(EGameStates.GAME, GetComponent<GameState>());
    }

    public void SwitchState(EGameStates state)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentState = GetGameState(state);
        currentState.OnEnter();
    }

    private BaseGameState GetGameState(EGameStates state)
    {
        gameStates.TryGetValue(state, out BaseGameState gameState);
        return gameState;
    }

    private BaseGameState GetState(EGameStates stateType)
    {
        if (gameStates == null) return null;
        
        gameStates.TryGetValue(stateType, out BaseGameState state);
        return state;
    }
}
