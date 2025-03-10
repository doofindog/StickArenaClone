using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixelArena.UI
{
    public class GameoverScreen : Screen
    {
        [SerializeField] private TMP_Text m_teamText;
        [SerializeField] private TMP_Text m_wonText;
        [SerializeField] private Animator m_anim;

        public override void Init()
        {

        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {

        }

        public void SetText(TeamType teamType)
        {
            if (m_teamText != null)
            {
                if (!m_teamText.TryGetComponent(out m_teamText))
                {
                    return;
                }
            }

            Team team = TeamManager.Instance.GetTeamFromType(teamType);
            m_teamText.color = team.color;
            m_teamText.text = teamType.ToString();

            m_anim.Play("ShowText");
        }
    }
}

