using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private const string LOADING_GAME_TEXT = "LOADING GAME";
    private const string WAITING_PLAYERS_TEXT = "WAITING FOR PLAYERS";
    
    [SerializeField] private GameObject startScreen;
    [SerializeField] private TMP_Text headingText;

    private void Awake()
    {
        ConnectionManager.Instance.playersConnected.OnValueChanged += (value, newValue) =>
        {
            headingText.text = WAITING_PLAYERS_TEXT + " " + $"{ConnectionManager.Instance.playersConnected.Value}/{ConnectionManager.Instance.MaxPlayers}";
            
            if (newValue >= ConnectionManager.Instance.MaxPlayers)
            {
                HandlePreGameText();
            }
        };
    }

    public void Disconnected()
    {
        ConnectionManager.TryDisconnect();
        startScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    private void HandlePreGameText()
    {
        headingText.text = LOADING_GAME_TEXT;
    }
}
