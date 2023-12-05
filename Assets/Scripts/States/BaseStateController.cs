using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateController
{
    protected CharacterState state = CharacterState.None;
    protected readonly StickManController stickManController;
    protected StateManager stateManager;

    protected BaseStateController(StickManController aircraft)
    {
        stickManController = aircraft;
    }

    public abstract void EnterState();
    public abstract void HandleUpdate();
    public abstract void HandleFixedUpdate();
    public abstract void ExitState();

    public virtual CharacterState GetState()
    {
        return state;
    }
}
