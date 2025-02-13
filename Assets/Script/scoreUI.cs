using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class scoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text greenScoreText;
    [SerializeField] private TMP_Text blueScoreText;
    [SerializeField] private TMP_Text yellowScoreText;

    private void Update()
    {
        blueScoreText.text = TeamManager.Instance.GetTeamFromType(TeamType.Blue).score.ToString();
        yellowScoreText.text = TeamManager.Instance.GetTeamFromType(TeamType.Yellow).score.ToString();
    }
}
