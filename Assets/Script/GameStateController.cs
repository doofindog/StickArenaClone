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
    private EGameStates currentStateType;
    private Dictionary<EGameStates, BaseGameState> gameStates = new Dictionary<EGameStates, BaseGameState>();

    protected override void Awake()
    {
        base.Awake();
        
        gameStates.Add(EGameStates.MENU, GetComponent<MenuState>());
        gameStates.Add(EGameStates.GAME, GetComponent<GameState>());
        gameStates.Add(EGameStates.OVER, GetComponent<EndState>());
    }

    public void SwitchState(EGameStates state)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }

        currentStateType = state;
        currentState = GetGameState(state);
        currentState.OnEnter();
        
    }
    
    public EGameStates GetState()
    {
        return currentStateType;
    }

    private BaseGameState GetGameState(EGameStates state)
    {
        gameStates.TryGetValue(state, out BaseGameState gameState);
        return gameState;
    }
}
