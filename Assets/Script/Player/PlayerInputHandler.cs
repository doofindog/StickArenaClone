using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

//Handles All the Data Being Passed through the Input Reader
public class PlayerInputHandler : NetworkBehaviour
{
    private CharacterDataHandler _dataHandler;
    [SerializeField] private InputReader _inputReader;
    

    public void Init(NetController netController)
    {
        _dataHandler = GetComponent<CharacterDataHandler>();
        
        if (IsClient && IsOwner)
        {
            _inputReader.MoveEvent += HandleMovePressed;
            _inputReader.AttackEvent += HandleAttackPressed;
            _inputReader.InteractEvent += HandleInteractPressed;
            _inputReader.ReloadEvent += HandleReloadPressed;
            _inputReader.DodgeEvent += HandleDodgePressed;
        }
    }

    private void HandleDodgePressed(bool pressed)
    {
        _dataHandler.dodgePressed = pressed;
    }

    private void HandleMovePressed(Vector2 direction)
    {
        _dataHandler.direction = direction;
    }
    
    private void HandleAttackPressed(bool pressed)
    {
        _dataHandler.attackPressed = pressed;
    }

    private void HandleInteractPressed(bool pressed)
    {
        _dataHandler.interactPressed = pressed;
    }

    private void HandleReloadPressed(bool pressed)
    {
        _dataHandler.reloadPressed = pressed;
    }
    
    public void Update()
    {
        if (IsClient && IsOwner)
        {
            float aimAngleTo360 = UpdateAim() < 0 ? 360 + UpdateAim() : UpdateAim();
            _dataHandler.aimAngle = aimAngleTo360;
        }
    }

    private float UpdateAim()
    {
        if (Camera.main != null)
        {
            Vector3 playerToMouseDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
            float angle = Mathf.Atan2(playerToMouseDirection.y, playerToMouseDirection.x) * Mathf.Rad2Deg;

            return angle;
        }
        else
        {
            Debug.Log("camera is null");
            return 0.0f;
        }
    }
}