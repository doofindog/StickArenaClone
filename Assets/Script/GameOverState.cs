using System.Collections;
using UnityEngine;
using PixelArena.UI;

public class GameOverState : BaseGameState
{
    public override void OnEnter()
    {
        UIManager.Instance.TvController.TurnOn(HandleShowResults, 1);
    }

    public override void OnExit()
    {
        UIManager.Instance.TvController.TurnOff();
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
