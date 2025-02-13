using System.Collections;
using System.Runtime.Serialization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuState : BaseGameState
{
    [SerializeField] public AudioClip menuMusic;
    [SerializeField] public AudioClip cassetteAudio;
    [SerializeField] public AudioClip turnOffAudio;
    [SerializeField] private Volume _postProcessVolume;

    private bool _isFirst;

    public void Awake()
    {
        _isFirst = true;
        CustomNetworkEvents.AllPlayersConnectedEvent += LoadToGame;
    }

    public override void OnEnter()
    {
#if SERVER
        TvController controller = UIManager.Instance.TvController;
        controller.gameObject.SetActive(false);
        ConnectionManager.Instance.StartServer();

#elif CLIENT
        AudioManager.Instance.GetSource().pitch = 1;
        CameraController.Instance.ChangeState(ECameraState.MENU);

        if (_postProcessVolume != null)
        {
            _postProcessVolume.profile.TryGet(out LensDistortion lensDistortion);
            _postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection);
            _postProcessVolume.profile.TryGet(out Bloom bloom);

            lensDistortion.active = true;
            paniniProjection.active = true;

            lensDistortion.intensity.value = 0.278f;
            lensDistortion.xMultiplier.value = 0.85f;
            lensDistortion.yMultiplier.value = 0.85f;
            paniniProjection.distance.value = 0.213f;
            bloom.intensity.value = 4.65f;
        }

        ShowScreen();
#endif
    }

    private void ShowScreen()
    {
        int audioDelay = GameManager.Instance.GetSessionSettings().cassetAudioTime;
        AudioManager.Instance.PlayOneShot(cassetteAudio, audioDelay);

        TvController tvController = UIManager.Instance.TvController;
        if(tvController)
        {
            int turnOnDelay = GameManager.Instance.GetSessionSettings().TurnOnScreenTime;
            tvController.TurnOn(HandleSplashCompleted, turnOnDelay);
        }
    }

    private void HandleSplashCompleted()
    {
        UIManager.Instance.ReplaceScreen(Screens.Menu);
        AudioManager.Instance.Play(menuMusic);
    }

    private void LoadToGame()
    {
        GameManager.Instance.SwitchState(EGameStates.GAME);
    }

    public override void OnExit()
    {
        UIManager.Instance.ReplaceScreen(Screens.None);

        void HandleServer()
        {

           
            
        }

        void HandleClient()
        {

        }

#if SERVER
        Debugger.Log($"{this.name} SERVER ON EXIT");

#elif CLIENT
        if (!IsClient) return;

            
        TvController tvController = UIManager.Instance.TvController;
        if (tvController != null)
        {
            tvController.TurnOff();
        }
        AudioManager.Instance.Stop();
        AudioManager.Instance.PlayOneShot(turnOffAudio);

#endif   
    }
}
