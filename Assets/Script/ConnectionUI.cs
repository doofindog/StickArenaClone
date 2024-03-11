using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private const string LOADING_GAME_TEXT = "LOADING GAME";
    private const string WAITING_PLAYERS_TEXT = "WAITING FOR PLAYERS";
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject connectionLayoutPanel; 
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text headingText;
    
    private void Awake()
    {
        ConnectionManager.Instance.playersConnected.OnValueChanged += UpdatePanel;
    }

    public void Update()
    {
        List<PlayerSessionData> playerCollection = ConnectionManager.Instance.GetPlayerSessionDataDict().Values.ToList();
        foreach (Transform children in connectionLayoutPanel.transform)
        {
                
            Destroy(children.gameObject);
        }
            
        foreach (PlayerSessionData data in playerCollection)
        {
            ConnectionCell cell = Instantiate(cellPrefab, connectionLayoutPanel.transform).GetComponent<ConnectionCell>();
            cell.UpdateCell(data);
        }
    }

    private void UpdatePanel(int value, int newValue)
    {
        headingText.text = WAITING_PLAYERS_TEXT + " " + $"{ConnectionManager.Instance.playersConnected.Value}/{ConnectionManager.Instance.MaxPlayers}";
            
        if (newValue >= ConnectionManager.Instance.MaxPlayers)
        {
            headingText.text = LOADING_GAME_TEXT;
        }
    }

    public void Disconnected()
    {
        ConnectionManager.TryDisconnect();
        menuPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
