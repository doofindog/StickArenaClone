using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameoverScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text teamText;
    [SerializeField] private TMP_Text wonText;
    [SerializeField] private Animator anim;

    public void OnEnable()
    {
        anim.Play("ShowText");
    }

    public void SetText(TeamType teamType)
    {
        if (teamText != null)
        {
            if(!teamText.TryGetComponent(out teamText))
            {
                return;
            }
        }

        Team team = TeamManager.Instance.GetTeamFromType(teamType);
        wonText.color = teamText.color = team.color;
        teamText.text = teamType.ToString() + " Team";
    }
}
