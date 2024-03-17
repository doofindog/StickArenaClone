using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameState : BaseGameState
{
    [SerializeField] private TvController tvController;

    public void Awake()
    {
        GameEvents.TeamWonEvent += StopGame;
    }

    public override void OnEnter()
    {
        if (NetworkManager.Singleton == null)
        {
            GameManager.Instance.ChangeState(EGameStates.MENU);
        }
        
        UIManager.Instance.ReplaceScreen(Screens.Game);
        if (IsServer)
        {
            StartCoroutine(StartGame());
        }
    }
    
    private IEnumerator StartGame()
    {
        SessionSettings sessionSettings = GameManager.Instance.GetSessionSettings();
        while (GameManager.Instance.prepTimer.Value < sessionSettings.prepGameTime)
        {
            yield return new WaitForSeconds(3);
            GameManager.Instance.prepTimer.Value++;
        }

        PreparingGameClientRPC();
        
        GameManager.Instance.startGameTimer.Value = sessionSettings.startGameTime;
        while (GameManager.Instance.startGameTimer.Value > 0)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.startGameTimer.Value--;
        }
        
        GameManager.Instance.SpawnAllPlayers();
        StartGameClientRPC();
    }
    
    private void StopGame(TeamType teamType)
    {
        StartCoroutine(SlowDownGame(teamType));
    }

    private IEnumerator SlowDownGame(TeamType teamType)
    {
        float timeScale = Time.timeScale;
        while (timeScale > 0f)
        {
            timeScale -= TickManager.Instance.GetMinTickTime();
            Time.timeScale = timeScale;

            yield return new WaitForSeconds(TickManager.Instance.GetMinTickTime());

            if (timeScale <= 0.2f)
            {
                GameEvents.SendGameOver(teamType);
            }
        }
    }
    
    [ClientRpc]
    private void  PreparingGameClientRPC()
    {
        tvController.TurnOn(GameEvents.SendPreparingArenaEvent);
    }
    
    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
    }
}
