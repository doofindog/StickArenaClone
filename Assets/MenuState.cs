using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuState : BaseGameState
{
    [SerializeField] private TvController controller;
    [SerializeField] public AudioClip menuMusic;
    [SerializeField] public AudioClip cassetteAudio;
    [SerializeField] public AudioClip turnOffAudio;

    private bool _isFirst;

    public void Awake()
    {
        _isFirst = true;
        CustomNetworkEvents.AllPlayersConnectedEvent += LoadToGame;
    }

    public override void OnEnter()
    {
        StartCoroutine(ShowScreen());
    }

    public void Update()
    {
        
    }

    private IEnumerator ShowScreen()
    {
        if (_isFirst)
        {
            _isFirst = false;
            yield return new WaitForSeconds(1);
            AudioManager.Instance.PlayOneShot(cassetteAudio);
            yield return new WaitForSeconds(2);
        }

        controller.TurnOn(HandleSplashCompleted);
    }

    private void HandleSplashCompleted()
    {
        UIManager.Instance.ReplaceScreen(Screens.Menu);
        AudioManager.Instance.Play(menuMusic);
        GameEvents.SendSplashCompleted();
    }

    private void LoadToGame()
    {
        GameManager.Instance.ChangeState(EGameStates.GAME);
    }

    public override void OnExit()
    {
        UIManager.Instance.ReplaceScreen(Screens.None);
        controller.TurnOf();
        AudioManager.Instance.Stop();
        AudioManager.Instance.PlayOneShot(turnOffAudio);
    }
}
