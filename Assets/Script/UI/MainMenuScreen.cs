using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject connectionPanel;

    public void Awake()
    {
        NetworkManager.Singleton.OnClientStarted += ChangeToConnectionPanel;
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
        ConnectionManager.TryStartHost(usernameField.text);
    }

    public void OnJoinPressed()
    {
        if (string.IsNullOrEmpty(usernameField.text))
        {
            PlayNoUserNameAnim();
            return;
        }
        ConnectionManager.TryJoin(usernameField.text);
    }

    private void ChangeToConnectionPanel()
    {
        connectionPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void PlayNoUserNameAnim()
    {
        animator.Play("usernameError");
    }
    
}


