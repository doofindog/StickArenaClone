using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameState : BaseGameState
{
    [SerializeField] private TvController tvController;
    
    public override void OnEnter()
    {
        if (NetworkManager.Singleton == null)
        {
            GameManager.Instance.ChangeState(GameStates.MENU);
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
            yield return new WaitForSeconds(1);
            GameManager.Instance.prepTimer.Value++;
        }

        tvController.TurnOn(PreparingGameClientRPC);
        
        GameManager.Instance.startGameTimer.Value = sessionSettings.startGameTime;
        while (GameManager.Instance.startGameTimer.Value > 0)
        {
            yield return new WaitForSeconds(1);
            GameManager.Instance.startGameTimer.Value--;
        }
        
        GameManager.Instance.SpawnAllPlayers();
        StartGameClientRPC();
    }
    
    [ClientRpc]
    private void  PreparingGameClientRPC()
    {
        GameEvents.SendPreparingArenaEvent();
    }
    
    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
    }
}
