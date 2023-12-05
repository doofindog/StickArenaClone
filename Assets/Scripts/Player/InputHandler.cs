using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

//Handles All the Data Being Passed through the Input Reader
public class InputHandler : NetworkBehaviour
{
    private StickManData _data;
    [SerializeField] private InputReader _inputReader;

    public void Init(StickManController player)
    {
        _data = player.GetData();
        if (IsClient && IsOwner)
        {
            _inputReader.MoveEvent += HandleMovePressed;
            _inputReader.AttackEvent += HandleAttackPressed;
            _inputReader.InteractEvent += HandleInteractPressed;
        }
    }

    private void HandleMovePressed(Vector2 direction)
    {
        _data.direction = direction;
    }
    
    private void HandleAttackPressed(bool pressed)
    {
        _data.attackPressed = pressed;
    }

    private void HandleInteractPressed(bool pressed)
    {
        _data.interactPressed = pressed;
    }

    public void Update()
    {
        if (IsClient && IsOwner)
        {
            _data.aimAngle = UpdateAim() - _data.aimOffset;
        }
    }

    private float UpdateAim()
    {
        Vector3 playerToMouseDirection = Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position);
        float angle = Mathf.Atan2(playerToMouseDirection.y, playerToMouseDirection.x) * Mathf.Rad2Deg;

        return angle;
    }
}