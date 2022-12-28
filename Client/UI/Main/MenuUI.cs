using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main
{
    public sealed class MenuUI : MonoBehaviour
    {
        [SerializeField] private Sprite safeToggleOnSprite, safeToggleOffSprite, ressToggleOnSprite, ressToggleOffSprite;
        [SerializeField] private Button questsButton, statsButton, safeToggleButton, ressToggleButton;

        private void Start()
        {
            questsButton.onClick.AddListener(ShowHideQuestsWindow);
            statsButton.onClick.AddListener(ShowHideStatsWindow);
            safeToggleButton.onClick.AddListener(SendToggleSafe);
            ressToggleButton.onClick.AddListener(SendToggleRess);
        }

        private static void ShowHideQuestsWindow()
        {
            UIManager.GameUI.QuestWindow.ShowWindow(false);
        }
        
        private static void ShowHideStatsWindow()
        {
            UIManager.GameUI.StatsWindow.ShowHideWindow();
        }

        public void ToggleSafe(bool safeToggleOn)
        {
            safeToggleButton.image.sprite = safeToggleOn ? safeToggleOnSprite : safeToggleOffSprite;
            string message = safeToggleOn ? Constants.SAFE_TOGGLE_ON : Constants.SAFE_TOGGLE_OFF;
            UIManager.GameUI.Console.WriteLine(message, ConsoleMessage.Warning);
        }

        public void ToggleRess(bool ressToggleOn)
        {
            ressToggleButton.image.sprite = ressToggleOn ? ressToggleOnSprite : ressToggleOffSprite;
            string message = ressToggleOn ? Constants.RESS_TOGGLE_ON : Constants.RESS_TOGGLE_OFF;
            UIManager.GameUI.Console.WriteLine(message, ConsoleMessage.Warning);
        }

        private static void SendToggleSafe()
        {
            PacketSender.PlayerInput(PlayerInput.SafeToggle);
        }

        private static void SendToggleRess()
        {
            PacketSender.PlayerInput(PlayerInput.RessToggle);
        }
    }
}
