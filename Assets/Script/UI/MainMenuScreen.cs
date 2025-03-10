using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace PixelArena.UI
{
    public class MainMenuScreen : Screen
    {
        [SerializeField] private TMP_InputField usernameField;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject connectionPanel;
        [SerializeField] private GameObject hostDisconnectedPopup;
        [SerializeField] private GameObject settingsPopUp;

        public override void Init()
        {
            NetworkManager.Singleton.OnClientStarted += ChangeToConnectionPanel;
            NetworkManager.Singleton.OnClientStopped += OnDisconnected;
            NetworkManager.Singleton.OnServerStarted += ChangeToConnectionPanel;
        }

        public override void OnEnter()
        {
            menuPanel.SetActive(true);
            connectionPanel.SetActive(false);
            hostDisconnectedPopup.SetActive(false);
            settingsPopUp.SetActive(false);
        }

        public override void OnExit()
        {
            menuPanel.SetActive(false);
            connectionPanel.SetActive(false);
            hostDisconnectedPopup.SetActive(false);
            settingsPopUp.SetActive(false);
        }

        public void OnHostPressed()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                PlayNoUserNameAnim();
                return;
            }
            if (usernameField.text.Contains(' '))
            {
                usernameField.text = usernameField.text.Replace(' ', '_');
            }

            ConnectionManager.Instance.StartServer(usernameField.text);
        }

        public void OnJoinPressed()
        {
            if (string.IsNullOrEmpty(usernameField.text))
            {
                PlayNoUserNameAnim();
                return;
            }

            menuPanel.SetActive(false);
            ConnectionManager.Instance.StartClient(usernameField.text);
        }


        private void ChangeToConnectionPanel()
        {
            Debugger.Log("[UI] Changing to Connection Panel");
            connectionPanel.SetActive(true);
            menuPanel.SetActive(false);
        }

        private void PlayNoUserNameAnim()
        {
            animator.Play("usernameError");
        }

        public void CloseHostPopup()
        {
            hostDisconnectedPopup.SetActive(false);
        }

        public void BackToMenu()
        {
            connectionPanel.SetActive(false);
            menuPanel.SetActive(true);
        }

        public void ToggleOption()
        {
            menuPanel.SetActive(!menuPanel.activeInHierarchy);
            settingsPopUp.SetActive(!settingsPopUp.activeInHierarchy);
        }

        private void OnDisconnected(bool obj)
        {
            menuPanel.SetActive(true);
            connectionPanel.SetActive(false);
        }

    }
}



