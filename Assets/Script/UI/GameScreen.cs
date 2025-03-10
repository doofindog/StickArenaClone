using UnityEngine;

namespace PixelArena.UI
{
    public class GameScreen : Screen
    {
        [SerializeField] private PreGameUI prePanel;
        [SerializeField] private DeathPanel deathPanel;
        [SerializeField] private scoreUI scoreUI;
        [SerializeField] private GameObject playerHealth;

        public override void Init()
        {
            PlayerEvents.PlayerDiedEvent += DisplayDeathScreen;
            GameEvents.PreparingArenaEvent += DisplayPreGameScreen;
            GameEvents.OnGameStartEvent += DisplayGameUI;
        }

        public override void OnEnter()
        {
            void HandleServer()
            {
                playerHealth.SetActive(false);
                prePanel.gameObject.SetActive(true);
            }

            void HandleClient()
            {
                playerHealth.SetActive(true);
            }

            deathPanel.gameObject.SetActive(false);
            scoreUI.gameObject.SetActive(true);

            GameUtilt.ExecuteNetworkCode(HandleServer, HandleClient);
        }

        public override void OnExit()
        {
            
        }

        private void DisplayGameUI()
        {
            void HandlerClient()
            {
                playerHealth.SetActive(true);
                scoreUI.gameObject.SetActive(true);
            }

            GameUtilt.ExecuteNetworkCode(null, HandlerClient);
        }

        private void DisplayDeathScreen()
        {
            deathPanel.gameObject.SetActive(true);
        }

        private void DisplayPreGameScreen()
        {
            prePanel.gameObject.SetActive(true);
        }

        public void OnDestroy()
        {
            PlayerEvents.PlayerDiedEvent -= DisplayDeathScreen;
        }
    }
}

