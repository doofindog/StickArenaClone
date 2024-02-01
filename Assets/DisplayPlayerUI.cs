using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class DisplayPlayerUI : MonoBehaviour
{
    public GameObject connectionLayoutPanel; 
    public GameObject cellPrefab;
    
    private bool _IsUpdating;

    public void Awake()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += DisplayPlayers;
    }

    private void OnDestroy()
    {
        StopCoroutine(UpdateUI());
    }

    private void DisplayPlayers(ulong clientID)
    {
        if(_IsUpdating == true) return;
        StartCoroutine(UpdateUI());
    }

    private IEnumerator UpdateUI()
    {
        _IsUpdating = true;
        while (true)
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

            yield return new WaitForSeconds(1);
        }
    }
}
