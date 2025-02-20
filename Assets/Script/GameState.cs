using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameState : BaseGameState
{
    [SerializeField] private GameObject defaultWeapon;
    [SerializeField] private Volume _postProcessVolume;
    [SerializeField] private AudioClip gameMusic;

    private int clientReadyCount;

    public void Awake()
    {
        GameEvents.TeamWonEvent += StopGame;
    }

    public override void OnEnter()
    {
        void HandleServer()
        {
            UIManager.Instance.ReplaceScreen(Screens.Game);
        }

        void HandleClient()
        {
            UIManager.Instance.ReplaceScreen(Screens.Game);
            //ScoreManager.Instance.Reset();

            if (_postProcessVolume != null)
            {
                if (_postProcessVolume.profile.TryGet(out LensDistortion lensDistortion))
                {
                    lensDistortion.active = false;
                }

                if (_postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection))
                {
                    paniniProjection.distance.value = 0.01f;
                    paniniProjection.cropToFit.value = 0.632f;
                }

                if (_postProcessVolume.profile.TryGet(out Bloom bloom))
                {
                    bloom.intensity.value = 2.0f;
                }
            }

            ClientConnectedToStateServerRPC();
        }

#if SERVER
        HandleServer();

#elif CLIENT
        HandleClient();
#elif UNITY_EDITOR
        GameManager.GameType gameType = GameManager.Instance.GetGameType();
        if (gameType == GameManager.GameType.SERVER)
        {
            HandleServer();
        }
        else if (gameType == GameManager.GameType.CLIENT)
        {
            HandleClient();
        }

#endif
    }

    public override void OnExit()
    {
        StopAllCoroutines();

        TvController tvController = UIManager.Instance.TvController;
        if (tvController != null)
        {
            tvController.TurnOff();
        }
    }

    private void TryStartGame()
    {
        bool canStartGame = SessionManager.Instance.clientDataCollection.Count == GameManager.Instance.GetSessionSettings().maxConnections;
        if (canStartGame)
        {
            StartCoroutine(StartGame());
        }
    }
    
    private IEnumerator StartGame()
    {

        GameSettings sessionSettings = GameManager.Instance.GetSessionSettings();

        yield return new WaitForSeconds(sessionSettings.countDownTime);

        SpawnManager.Instance.SpawnAllPlayers();
        List<NetworkClient> clients = NetworkManager.ConnectedClientsList.ToList();
        foreach (NetworkClient client in clients)
        {
            client.PlayerObject.GetComponent<WeaponComponent>().GiveDefaultWeapon();
        }

        PreparingGameClientRPC();

        while (SessionManager.Instance.countDownTimer.Value > 0)
        {
            yield return new WaitForSeconds(1);
            SessionManager.Instance.countDownTimer.Value--;
        }
        
        StartGameClientRPC();
    }
    
    private void StopGame(TeamType teamType)
    {
        StartCoroutine(SlowDownGame());
    }

    private IEnumerator SlowDownGame()
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
                GameEvents.SendGameOver();
                break;
            }
        }
        
        Time.timeScale = 1;
        AudioManager.Instance.Stop();
        GameManager.Instance.SwitchState(EGameStates.OVER);
    }
    
    [ClientRpc]
    private void  PreparingGameClientRPC()
    {
        TvController tvController = UIManager.Instance.TvController;
        tvController.TurnOn(GameEvents.SendPreparingArenaEvent);
    }
    
    [ClientRpc]
    private void StartGameClientRPC()
    {
        Debug.Log("Client rpc Called");

        GameEvents.SendStartGameEvent();

        if (gameMusic != null)
        {
            AudioManager.Instance.Play(gameMusic);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedToStateServerRPC()
    {
        TryStartGame();
    }    
}
