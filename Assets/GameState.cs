using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameState : BaseGameState
{
    [SerializeField] private TvController _tvController;
    [SerializeField] private Volume _postProcessVolume;
    [SerializeField] private AudioClip gameMusic;

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

        if (_postProcessVolume != null)
        {
            _postProcessVolume.profile.TryGet(out ChromaticAberration chromaticAberration);
            _postProcessVolume.profile.TryGet(out LensDistortion lensDistortion);
            _postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection);
            _postProcessVolume.profile.TryGet(out Bloom bloom);

            chromaticAberration.intensity.value = 0.235f;
            lensDistortion.intensity.value = -0.6f;
            lensDistortion.xMultiplier.value = 0.0f;
            lensDistortion.yMultiplier.value = 0.0f;
            paniniProjection.distance.value = 0.027f;
            paniniProjection.cropToFit.value = 0.632f;
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
            AudioManager.Instance.GetSource().pitch -= TickManager.Instance.GetMinTickTime();
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
        _tvController.TurnOn(GameEvents.SendPreparingArenaEvent);
    }
    
    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
        
        if (gameMusic != null)
        {
            AudioManager.Instance.Play(gameMusic);
        }
        
    }
}
