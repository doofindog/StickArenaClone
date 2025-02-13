using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private const string LOADING_GAME_TEXT = "LOADING GAME";
    private const string JOINING_GAME_TEXT = "JOINING GAME";
    
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private GameObject connectionLayoutPanel; 
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TMP_Text m_connectionCodeText;
    
    private void Awake()
    {

    }

    private void OnEnable()
    {
        UpdatedHeadingText(JOINING_GAME_TEXT);
    }

    public void Update()
    {
        UpdatePlayerTable();
    }

    public void UpdatePlayerTable()
    {
        if(NetworkManager.Singleton == null) return;

        NetworkList<PublicPlayerData> playerCollection = SessionManager.Instance.publicPlayerDataCollection;
        if (playerCollection == null || playerCollection.Count == 0)
        {
            return;
        }

        foreach (Transform children in connectionLayoutPanel.transform)
        {
            Destroy(children.gameObject);
        }

        foreach (PublicPlayerData data in playerCollection)
        {
            ConnectionCell cell = Instantiate(cellPrefab, connectionLayoutPanel.transform).GetComponent<ConnectionCell>();
            cell.UpdateCell(data);
        }
    }

    private void UpdatedHeadingText(string pUpdatedText)
    {
        if (m_connectionCodeText == null)
        {
            return;
        }

        m_connectionCodeText.text = string.IsNullOrEmpty(pUpdatedText) ? m_connectionCodeText.text : pUpdatedText;
    }

    public void Disconnected()
    {
        ConnectionManager.Instance.TryDisconnect();
        
        menuPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    
}
