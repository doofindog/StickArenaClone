using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private const string LOADING_GAME_TEXT = "LOADING GAME";
    private const string WAITING_PLAYERS_TEXT = "WAITING FOR PLAYERS";
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject connectionLayoutPanel; 
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text connectionCodeText;
    
    private void Awake()
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        connectionManager.playersConnected.OnValueChanged += UpdatePanel;
    }

    public void Update()
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        List<PlayerData> playerCollection = connectionManager.GetPlayerSessionDataDict().Values.ToList();
        foreach (Transform children in connectionLayoutPanel.transform)
        {
            Destroy(children.gameObject);
        }
            
        foreach (PlayerData data in playerCollection)
        {
            ConnectionCell cell = Instantiate(cellPrefab, connectionLayoutPanel.transform).GetComponent<ConnectionCell>();
            cell.UpdateCell(data);
        }

        SessionData sessionData = GameManager.Instance.connectionManager.GetSessionData();
        if (sessionData != null && !string.IsNullOrEmpty(sessionData.joinCode))
        {
            connectionCodeText.text = sessionData.joinCode;
        }
    }

    private void UpdatePanel(int value, int newValue)
    {
        ConnectionManager connectionManager = GameManager.Instance.connectionManager;
        headingText.text = WAITING_PLAYERS_TEXT + " " + $"{connectionManager.playersConnected.Value}/{connectionManager.MaxPlayers}";
        
        if (newValue >= connectionManager.MaxPlayers)
        {
            headingText.text = LOADING_GAME_TEXT;
        }
    }

    public void Disconnected()
    {
        GameManager.Instance.TryDisconnect();
        
        menuPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
