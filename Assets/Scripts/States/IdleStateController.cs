using Unity.Netcode;
using UnityEngine;

public class IdleStateController : BaseStateController
{
    private CharacterController _controller;

    public IdleStateController(PixelManController aircraft) : base(aircraft)
    {
        state = CharacterState.Idle;
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
