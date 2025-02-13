using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class GameOverState : BaseGameState
{
    public override void OnEnter()
    {
        StartCoroutine(TurnTvOn());
    }

    public override void OnExit()
    {
        TvController tvController = UIManager.Instance.TvController;
        if (tvController != null)
        {
            tvController.TurnOff();
        }
    }

    private IEnumerator TurnTvOn()
    {
        yield return new WaitForSeconds(1);
        TvController tvController = UIManager.Instance.TvController;
        if(tvController != null)
        {
            tvController.TurnOn(HandleShowResults);
        }
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
        ConnectionManager.Instance.TryDisconnect();
        ObjectPool.Instance.ClearPool();
        GameManager.Instance.SwitchState(EGameStates.MENU);
    }
}
