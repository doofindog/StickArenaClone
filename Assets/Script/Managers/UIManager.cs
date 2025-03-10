using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace PixelArena.UI
{
    public enum Screens
    {
        Menu = 0,
        Game = 1,
        GameOver = 2,
        None = 3,
    }

    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private Canvas m_gameCanvas;
        [SerializeField] private Canvas m_tvCanvas;
        [SerializeField] private Camera m_uiCamera;
        [SerializeField] private Screen[] m_screens;
        [SerializeField] private TvController m_tvController;

        private Screen m_currentScreen;

        public TvController TvController => m_tvController;

        public void Init()
        {
            m_tvController ??= FindAnyObjectByType<TvController>();
            if(m_tvController !=null)
            {
                m_tvController.gameObject.SetActive(true);
            }
            else
            {
                Debugger.Log("TV Controller not present", Debugger.DebugType.UI);
            }

            GameEvents.TeamWonEvent += TeamWonEvent;

            foreach (var screen in m_screens)
            {
                screen.Init();
            }

            m_gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            if (m_uiCamera != null)
            {
                m_gameCanvas.worldCamera = m_uiCamera;
            }

            ReplaceScreen(Screens.Menu);
        }

        private void TeamWonEvent(TeamType obj)
        {
            m_gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            if (m_uiCamera != null)
            {
                m_gameCanvas.worldCamera = m_uiCamera;
            }
        }

        public Screen ReplaceScreen(Screens screenEnum)
        {
            if(m_currentScreen != null)
            {
                m_currentScreen.OnExit();
                m_currentScreen.gameObject.SetActive(false);
            }

            if (m_screens is { Length: > 0 } && (int)screenEnum < m_screens.Length)
            {
                m_currentScreen = m_screens[(int)screenEnum];
                m_currentScreen.gameObject.SetActive(true);
                m_currentScreen.OnEnter();

                return m_screens[(int)screenEnum];
            }

            return null;
        }

        public T GetScreen<T>(Screens screenEnum)
        {
            return m_screens[(int)screenEnum].GetComponent<T>();
        }
    }
}
