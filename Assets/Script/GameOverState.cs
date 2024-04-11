using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverState : BaseGameState
{
    [SerializeField] private TvController _controller;
    public override void OnEnter()
    {
        StartCoroutine(TurnTvOn());
    }

    public override void OnExit()
    {
        _controller.TurnOff();
    }

    private IEnumerator TurnTvOn()
    {
        yield return new WaitForSeconds(1);
        _controller.TurnOn(HandleShowResults);
    }

    private void HandleShowResults()
    {
        GameoverScreen screen = UIManager.Instance.ReplaceScreen(Screens.GameOver).GetComponent<GameoverScreen>();
        TeamType t = ScoreManager.Instance.GetWinningTeam();
        screen.SetText(t);
        
        StartCoroutine(ExitGame());
    }

    private IEnumerator ExitGame()
    {
        yield return new WaitForSeconds(5);
        GameManager.Instance.TryDisconnect();
        ObjectPool.Instance.ClearPool();
        GameManager.Instance.SwitchState(EGameStates.MENU);
    }
}
