using Unity.Netcode;
using UnityEngine;

public class IdleStateController : BaseStateController
{
    private PlayerController _controller;

    public IdleStateController(PlayerController aircraft) : base(aircraft)
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
