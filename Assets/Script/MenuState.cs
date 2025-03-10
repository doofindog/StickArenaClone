using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using PixelArena.UI;

#if UNITY_EDITOR
using Unity.Multiplayer.Playmode;
#endif

public class MenuState : BaseGameState
{
    [SerializeField] private AudioClip m_menuMusic;
    [SerializeField] private AudioClip m_cassetteAudio;
    [SerializeField] private AudioClip m_turnOffAudio;
    [SerializeField] private Volume m_postProcessVolume;

    public override void OnEnter()
    {
        void HandleServer()
        {
            TvController controller = UIManager.Instance.TvController;
            controller.gameObject.SetActive(false);
        }

        void HandleClient()
        {
            AudioManager.Instance.GetSource().pitch = 1;
            CameraController.Instance.ChangeState(ECameraState.MENU);

            if (m_postProcessVolume != null)
            {
                m_postProcessVolume.profile.TryGet(out LensDistortion lensDistortion);
                m_postProcessVolume.profile.TryGet(out PaniniProjection paniniProjection);
                m_postProcessVolume.profile.TryGet(out Bloom bloom);

                lensDistortion.active = true;
                paniniProjection.active = true;

                lensDistortion.intensity.value = 0.278f;
                lensDistortion.xMultiplier.value = 0.85f;
                lensDistortion.yMultiplier.value = 0.85f;
                paniniProjection.distance.value = 0.213f;
                bloom.intensity.value = 4.65f;
            }

            ShowScreen();
        }

        GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
    }

    private void ShowScreen()
    {
        int audioDelay = GameManager.Instance.GetSessionSettings().cassetAudioTime;
        AudioManager.Instance.PlayOneShot(m_cassetteAudio, audioDelay);

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
        AudioManager.Instance.Play(m_menuMusic);
    }

    public override void OnExit()
    {
        void HandleServer()
        {
            UIManager.Instance.ReplaceScreen(Screens.None);
        }

        void HandleClient()
        {
            UIManager.Instance.ReplaceScreen(Screens.None);

            TvController tvController = UIManager.Instance.TvController;
            if (tvController != null)
            {
                tvController.TurnOff();
            }
            AudioManager.Instance.Stop();
            AudioManager.Instance.PlayOneShot(m_turnOffAudio);
        }

#if SERVER
        HandleServer();

#elif CLIENT
        HandleClient()

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
}
