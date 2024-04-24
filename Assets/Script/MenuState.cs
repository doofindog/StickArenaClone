using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuState : BaseGameState
{
    [SerializeField] private TvController controller;
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
        AudioManager.Instance.GetSource().pitch = 1;
        CameraController.Instance.ChangeState(ECameraState.MENU);
        
        if (_postProcessVolume != null)
        {
            //_postProcessVolume.profile.TryGet(out ChromaticAberration chromaticAberration);
            _postProcessVolume.profile.TryGet(out LensDistortion lensDistortion);
            _postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection);
            _postProcessVolume.profile.TryGet(out Bloom bloom);

            lensDistortion.active = true;
            paniniProjection.active = true;
            //chromaticAberration.active = true;

            //chromaticAberration.intensity.value = 0.4f;
            lensDistortion.intensity.value = 0.278f;
            lensDistortion.xMultiplier.value = 0.85f;
            lensDistortion.yMultiplier.value = 0.85f;
            paniniProjection.distance.value = 0.213f;
            bloom.intensity.value = 4.65f;
        }
        
        StartCoroutine(ShowScreen());
    }

    public void Update()
    {
        
    }

    private IEnumerator ShowScreen()
    {
        yield return new WaitForSeconds(1);
        AudioManager.Instance.PlayOneShot(cassetteAudio);
        yield return new WaitForSeconds(2);

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
        GameManager.Instance.SwitchState(EGameStates.GAME);
    }

    public override void OnExit()
    {
        UIManager.Instance.ReplaceScreen(Screens.None);
        controller.TurnOff();
        AudioManager.Instance.Stop();
        AudioManager.Instance.PlayOneShot(turnOffAudio);
    }
}
