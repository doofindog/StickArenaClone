using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ConnectionCell : MonoBehaviour
{
    public TMP_Text usernameText;

    public void UpdateCell(PlayerSessionData sessionData)
    {
        usernameText.text = sessionData.userName;
    }
}
