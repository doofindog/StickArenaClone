public class StateManager
{
    private BaseStateController _currentStateController;
    private IdleStateController _idleStateController;
    private JogStateController _jogStateController;

    public void Init(PlayerController aircraft)
    {
        _idleStateController = new IdleStateController(aircraft);
        _jogStateController = new JogStateController(aircraft);
        
        SwitchState(CharacterState.Idle);
    }

    public void SwitchState(CharacterState nextState)
    {
        if (_currentStateController != null)
        {
            if (_currentStateController.GetState() == nextState)
            {
                return;
            }
            
            _currentStateController.ExitState();
        }
        
        _currentStateController = GetStateController(nextState);
        _currentStateController.EnterState();
    }
    
    public BaseStateController GetCurrentStateController()
    {
        return _currentStateController;
    }

    public CharacterState GetCurrentState()
    {
        return _currentStateController.GetState();
    }

    private BaseStateController GetStateController(CharacterState state)
    {
        switch (state)
        {
            case CharacterState.None:
                return _currentStateController;
            case CharacterState.Idle:
                return _idleStateController;
            case CharacterState.Jog:
                return _jogStateController;
            case CharacterState.Sprint:
                break;
            case CharacterState.Dead:
                break;
        }

        return null;
    }
}
