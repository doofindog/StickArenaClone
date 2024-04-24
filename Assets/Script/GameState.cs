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
            GameManager.Instance.SwitchState(EGameStates.MENU);
        }
        
        UIManager.Instance.ReplaceScreen(Screens.Game);
        ScoreManager.Instance.Reset();

        if (_postProcessVolume != null)
        {
            //_postProcessVolume.profile.TryGet(out ChromaticAberration chromaticAberration);
            _postProcessVolume.profile.TryGet(out LensDistortion lensDistortion);
            _postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection);
            _postProcessVolume.profile.TryGet(out Bloom bloom);

            //chromaticAberration.active = false;
            lensDistortion.active = false;
            paniniProjection.distance.value = 0.01f;
            paniniProjection.cropToFit.value = 0.632f;
            if (bloom != null) bloom.intensity.value = 2.0f;
        }
        
        if (IsServer)
        {
            StartCoroutine(StartGame());
        }
    }
    
    public override void OnExit()
    {
        StopAllCoroutines();
        _tvController.TurnOff();
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

        SpawnManager.Instance.SpawnAllPlayers();
        List<NetworkClient> clients = NetworkManager.ConnectedClientsList.ToList();
        foreach (NetworkClient client in clients)
        {
            NetworkObject weaponNetObj = SpawnManager.Instance.SpawnObject(defaultWeapon, SpawnManager.SpawnType.NETWORK).GetComponent<NetworkObject>();
            weaponNetObj.Spawn();
            WeaponComponent weaponComponent = client.PlayerObject.GetComponent<WeaponComponent>();
            weaponComponent.EquipWeapon(weaponNetObj.GetComponent<Weapon>());
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
        _tvController.TurnOn(GameEvents.SendPreparingArenaEvent);
    }
    
    [ClientRpc]
    private void StartGameClientRPC()
    {
        GameEvents.SendStartGameEvent();
        ArenaManager.Instance.ChangeState(ArenaManager.States.Idle);
        
        if (gameMusic != null)
        {
            AudioManager.Instance.Play(gameMusic);
        }
    }
}
