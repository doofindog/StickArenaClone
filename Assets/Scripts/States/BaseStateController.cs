using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStateController
{
    protected CharacterState state = CharacterState.None;
    protected readonly PixelManController PixelManController;
    protected StateManager stateManager;

    protected BaseStateController(PixelManController aircraft)
    {
        PixelManController = aircraft;
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
