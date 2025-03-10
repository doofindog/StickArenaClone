using System.Collections;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using PixelArena.UI;

public class GameState : BaseGameState
{
    [SerializeField] private Volume m_postProcessVolume;
    [SerializeField] private AudioClip gameMusic;

    private bool m_gameStarted;
    public bool GameStarted => m_gameStarted;
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
            if (m_postProcessVolume != null)
            {
                if (m_postProcessVolume.profile.TryGet(out LensDistortion lensDistortion))
                {
                    lensDistortion.active = false;
                }

                if (m_postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection))
                {
                    paniniProjection.distance.value = 0.253f;
                    paniniProjection.cropToFit.value = 1.0f;
                }

                if (m_postProcessVolume.profile.TryGet(out Bloom bloom))
                {
                    bloom.intensity.value = 2.0f;
                }
            }

            ClientConnectedToStateServerRPC();
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
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
    
    private IEnumerator StartGame()
    {
        GameSettings sessionSettings = GameManager.Instance.GetSessionSettings();

        yield return new WaitForSeconds(sessionSettings.countDownTime);

        SpawnManager.Instance.SpawnAllPlayers();

        PreparingGameClientRPC();

        while (SessionManager.Instance.countDownTimer.Value > 0)
        {
            yield return new WaitForSeconds(1);
            SessionManager.Instance.countDownTimer.Value--;
        }
        
        StartGameClientRPC();
        GameEvents.SendStartGameEvent();
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
        GameEvents.SendStartGameEvent();

        if (gameMusic != null)
        {
            AudioManager.Instance.Play(gameMusic);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ClientConnectedToStateServerRPC()
    {
        bool sessionCountCheck = SessionManager.Instance.GetConnectedClientCount() == GameManager.Instance.GetSessionSettings().maxConnections;
        bool canStartGame = sessionCountCheck && m_gameStarted == false;
        if (canStartGame)
        {
            m_gameStarted = true;
            StartCoroutine(StartGame());
        }
    }    
}
