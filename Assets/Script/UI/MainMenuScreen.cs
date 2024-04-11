using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private GameObject hostDisconnectedPopup;
    [FormerlySerializedAs("SettingsPopUp")] [SerializeField] private GameObject settingsPopUp;

    public void Awake()
    {
        CustomNetworkEvents.NetworkStartedEvent += ChangeToConnectionPanel;
        CustomNetworkEvents.DisconnectedEvent += OnDisconnected;
    }

    public void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    public void OnEnable()
    {
        menuPanel.SetActive(true);
        connectionPanel.SetActive(false);
    }

    public void OnHostPressed()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            PlayNoUserNameAnim();
            return;
        }
        
        GameManager.Instance.TryStartHost(usernameField.text);
    }

    public void OnJoinPressed()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            PlayNoUserNameAnim();
            return;
        }
        
        menuPanel.SetActive(false);
        joinPanel.SetActive(true);
    }

    public void OnJoinWithCodePressed()
    {
        GameManager.Instance.TryJoin(usernameField.text, joinCode.text);
    }

    private void ChangeToConnectionPanel()
    {
        Debugger.Log("[UI] Changing to Connection Panel");
        connectionPanel.SetActive(true);
        menuPanel.SetActive(false);
        joinPanel.SetActive(false);
    }

    private void PlayNoUserNameAnim()
    {
        animator.Play("usernameError");
    }

    private void OnDisconnected()
    {
        menuPanel.SetActive(true);
        connectionPanel.SetActive(false);
    }

    private void OnClientDisconnected(ulong clientID)
    {
        if (clientID == 0 && NetworkManager.Singleton.LocalClientId != 0)
        {
            hostDisconnectedPopup.SetActive(true);
        }
    }

    public void CloseHostPopup()
    {
        hostDisconnectedPopup.SetActive(false);
    }

    public void BackToMenu()
    {
        joinPanel.SetActive(false);
        connectionPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    public void ToggleOption()
    {
        menuPanel.SetActive(!menuPanel.activeInHierarchy);
        settingsPopUp.SetActive(!settingsPopUp.activeInHierarchy);
    }
}


