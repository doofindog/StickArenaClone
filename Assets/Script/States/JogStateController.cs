using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JogStateController : BaseStateController
{
    private CharacterController _controller;

    private int _tick;
    private float _tickRate = 1f / 60f;
    private float _tickDeltaTime = 0f;

    private const int BUFFER_SIZE = 1024;
    
    public JogStateController(PixelManController aircraft) : base(aircraft)
    {
        state = CharacterState.Jog;
        _controller = aircraft.GetComponent<CharacterController>();
    }
    
    public override void EnterState()
    {
        Debug.Log("Entering "+ state +" State");
    }

    public override void HandleUpdate()
    {
        
    }

    public override void HandleFixedUpdate()
    {

    }

    public override void ExitState()
    {
        
    }
}
