using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectionCell : MonoBehaviour
{
    public TMP_Text usernameText;
    public Color teamColour;

    public void UpdateCell(PublicPlayerData data)
    {
        usernameText.text = data.username.ToString().Replace('_',' ');

        teamColour = TeamManager.Instance.GetTeamData(data.teamType).color;
        usernameText.color = teamColour;
    }
}
