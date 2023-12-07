using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Input/Create Input Reader")]
public class InputReader : ScriptableObject, CharacterInput.IGameplayActions
{
    private CharacterInput _playerInput;
    
    private void OnEnable()
    {
        if (_playerInput == null)
        {
            _playerInput = new CharacterInput();
            _playerInput.Gameplay.SetCallbacks(this);
            
            _playerInput.Gameplay.Enable();
        }
    }

    public event Action<Vector2> MoveEvent;
    public event Action<bool> AttackEvent;
    public event Action<bool> InteractEvent;
    public event Action<bool> ReloadEvent;
    
    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            AttackEvent?.Invoke(true);
        }
        else if(context.canceled)
        {
            AttackEvent?.Invoke(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractEvent?.Invoke(true);
        }
        else if(context.canceled)
        {
            InteractEvent?.Invoke(false);
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ReloadEvent?.Invoke(true);
        }
        else if(context.canceled)
        {
            ReloadEvent?.Invoke(false);
        }
    }
}
