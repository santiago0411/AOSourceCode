using AOClient.Core;
using AOClient.Network;
using AOClient.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Party
{
    public class PartyMemberUI : MonoBehaviour, IPoolObject
    {
        public PlayerManager Player { get; private set; }
        public int InstanceId => Player.GetInstanceID();
        public bool IsBeingUsed => gameObject.activeSelf;

        [SerializeField] private TextMeshProUGUI usernameField;
        [SerializeField] private TextMeshProUGUI baseXpPercentField;
        [SerializeField] private TMP_InputField xpPercentInputField;
        [SerializeField] private TextMeshProUGUI totalXpGained;
        [SerializeField] private Button kickPlayerButton;
        
        // This variable is used to keep track of the total experience gained;
        // That way every time experience is gained the string being displayed doesn't need to be parsed back to an int
        // to calculate the total experience.
        private uint experienceGained;
        
        public void Init(PlayerManager member, bool localPlayerIsLeader)
        {
            Player = member;
            usernameField.text = member.Username;
            xpPercentInputField.onValidateInput = ValidateInput;
            
            SetPercentage(100);

            xpPercentInputField.text = "100";
            xpPercentInputField.gameObject.SetActive(false);

            experienceGained = 0;
            totalXpGained.text = "0";
            totalXpGained.gameObject.SetActive(true);
            
            kickPlayerButton.onClick.AddListener(OnKickPlayerButtonClick);
            // Do not show the kick button when the local player isn't the party leader
            // Or when this UI instance's player is the local player (avoid kicking themselves)
            bool showKickButton = localPlayerIsLeader && Player.Id != GameManager.Instance.LocalPlayer.Id;
            kickPlayerButton.gameObject.SetActive(showKickButton);
            
            gameObject.SetActive(true);
        }

        public void SetPercentageInputActive(bool active)
        {
            baseXpPercentField.gameObject.SetActive(!active);
            xpPercentInputField.gameObject.SetActive(active);
        }

        public byte GetCustomPercentageValue()
        {
            byte.TryParse(xpPercentInputField.text, out var value);
            return value;
        }

        public void SetPercentage(byte percentage)
        {
            baseXpPercentField.text = $"{percentage}%";
            baseXpPercentField.gameObject.SetActive(true);
        }

        public void SetKickButtonActive()
        {
            kickPlayerButton.gameObject.SetActive(true);
        }

        public void AddGainedExperience(uint experience)
        {
            experienceGained += experience;
            totalXpGained.text = experienceGained.ToString();
        }
        
        public void ResetPoolObject()
        {
            gameObject.SetActive(false);
        }

        private void OnKickPlayerButtonClick()
        {
            PacketSender.KickPartyMember(Player.Id);
        }

        private static char ValidateInput(string text, int charIndex, char addedChar)
        {
            return char.IsDigit(addedChar) ? addedChar : (char)0;
        }
    }
}