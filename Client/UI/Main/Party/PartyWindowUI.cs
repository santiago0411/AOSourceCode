using System.Collections.Generic;
using System.Linq;
using AO.Core.Ids;
using AOClient.Core;
using AOClient.Core.Utils;
using AOClient.Network;
using AOClient.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Party
{
    public class PartyWindowUI : MonoBehaviour
    {
        [SerializeField] private Button closeButton, editPercentagesButton, confirmPercentagesButton;
        [SerializeField] private RectTransform bonusPanel;
        [SerializeField] private TextMeshProUGUI bonusValueField;
        [SerializeField] private RectTransform labelsPanel;
        [SerializeField] private Button createPartyButton, inviteToPartyButton, leavePartyButton;
        [SerializeField] private RectTransform partyMembersPanel;
        [SerializeField] private PartyMemberUI partyMemberUIPrefab;

        private Pool<PartyMemberUI> partyMembersUIPools;
        private bool localPlayerIsLeader;

        private void Awake()
        {
            closeButton.onClick.AddListener(CloseWindow);
            editPercentagesButton.onClick.AddListener(OnEditPercentagesButtonClick);
            confirmPercentagesButton.onClick.AddListener(OnConfirmPercentagesButtonClick);
            createPartyButton.onClick.AddListener(OnCreatePartyButtonClick);
            inviteToPartyButton.onClick.AddListener(OnInviteToPartyButtonClick);
            leavePartyButton.onClick.AddListener(OnLeavePartyButtonClick);
            
            ResetWindow();
            SetBonus(0);
            
            partyMembersUIPools = new Pool<PartyMemberUI>(partyMemberUIPrefab, 5);
            gameObject.SetActive(false);
        }

        private void ResetWindow()
        {
            editPercentagesButton.gameObject.SetActive(false);
            confirmPercentagesButton.gameObject.SetActive(false);
            createPartyButton.gameObject.SetActive(true);
            inviteToPartyButton.gameObject.SetActive(false);
            leavePartyButton.gameObject.SetActive(false);
            labelsPanel.gameObject.SetActive(false);
            bonusPanel.gameObject.SetActive(false);
        }

        public void ShowWindow()
        {
            gameObject.SetActive(true);
        }

        private void CloseWindow()
        {
            gameObject.SetActive(false);
        }

        private static void OnCreatePartyButtonClick()
        {
            PacketSender.PlayerInput(PlayerInput.StartParty);
        }

        private void OnInviteToPartyButtonClick()
        {
            GameManager.Instance.LocalPlayer.SetClickRequest(ClickRequest.InviteToParty);
            CloseWindow();
        }

        private void OnLeavePartyButtonClick()
        {
            PacketSender.PlayerInput(PlayerInput.LeaveParty);
            ResetWindow();
            partyMembersUIPools.ResetObjects();
        }
        
        private void OnEditPercentagesButtonClick()
        {
            partyMembersUIPools.ForEachActiveObject(partyMember => partyMember.SetPercentageInputActive(true));
            editPercentagesButton.gameObject.SetActive(false);
            confirmPercentagesButton.gameObject.SetActive(true);
        }

        private void OnConfirmPercentagesButtonClick()
        {
            var playerPercentages = new List<(ClientId, byte)>(partyMembersUIPools.ActiveObjectsCount);
            partyMembersUIPools.ForEachActiveObject((partyMember, list) =>
            {
                list.Add((partyMember.Player.Id, partyMember.GetCustomPercentageValue()));
                partyMember.SetPercentageInputActive(false);
            }, playerPercentages);
            PacketSender.ChangePartyPercentages(playerPercentages);
            editPercentagesButton.gameObject.SetActive(true);
            confirmPercentagesButton.gameObject.SetActive(false);
        }
        
        public void OnLocalPlayerJoinedParty(ClientId leaderClientId, bool canEditPercentages, List<PlayerManager> members)
        {
            var localPlayer = GameManager.Instance.LocalPlayer;
            localPlayerIsLeader = localPlayer.Id == leaderClientId;
            members.Insert(0, localPlayer);

            foreach (var member in members)
                AddMember(member);

            OnCanEditPercentagesChanged(canEditPercentages);
            
            // Only display the joined message if the player actually joined a party and didn't just create one
            if (members.Count > 1)
                UIManager.GameUI.Console.WriteLine(Constants.YOU_JOINED_PARTY);
            
            createPartyButton.gameObject.SetActive(false);
            inviteToPartyButton.gameObject.SetActive(localPlayerIsLeader);
            leavePartyButton.gameObject.SetActive(true);
            labelsPanel.gameObject.SetActive(true);
        }

        public void OnPlayerJoinedParty(PlayerManager player)
        {
            AddMember(player);
            UIManager.GameUI.Console.WriteLine(Constants.PlayerJoinedParty(player.Username));
        }

        public void OnPlayerLeftParty(PlayerManager player, bool kicked)
        {
            if (player == GameManager.Instance.LocalPlayer)
            {
                var message = kicked ? Constants.YOU_HAVE_BEEN_KICKED : Constants.YOU_LEFT_PARTY;
                UIManager.GameUI.Console.WriteLine(message);
                partyMembersUIPools.ResetObjects();
                ResetWindow();
                return;
            }
            
            UIManager.GameUI.Console.WriteLine(Constants.PlayerLeftParty(player.Username));
            var partyMemberUI = partyMembersUIPools.FindObject(player.GetInstanceID());
            partyMemberUI.ResetPoolObject();
        }
        
        public void OnCanEditPercentagesChanged(bool canEditPercentages)
        {
            editPercentagesButton.gameObject.SetActive(canEditPercentages);
        }
        
        public void OnExperiencePercentagesChanged(Dictionary<PlayerManager, byte> playerPercentages, byte percentageBonus)
        {
            SetBonus(percentageBonus);
            partyMembersUIPools.ForEachActiveObject((partyMember, percentages) =>
                partyMember.SetPercentage(percentages[partyMember.Player]), playerPercentages);
        }

        public void OnPartyLeaderChanged(PlayerManager leader)
        {
            if (leader == GameManager.Instance.LocalPlayer)
            {
                UIManager.GameUI.Console.WriteLine(Constants.YOU_ARE_NOW_PARTY_LEADER);
                partyMembersUIPools.ForEachActiveObject(partyMember => partyMember.SetKickButtonActive());
                inviteToPartyButton.gameObject.SetActive(true);
                localPlayerIsLeader = true;
                return;
            }
            
            UIManager.GameUI.Console.WriteLine(Constants.PlayerIsNowPlayerLeader(leader.Username));
        }

        public void OnPartyGainedExperience(PlayerManager[] members, uint experience)
        {
            partyMembersUIPools.ForEachActiveObject((partyMember, state) =>
            {
                var (players, xp) = state;
                if (players.Contains(partyMember.Player))
                    partyMember.AddGainedExperience(xp);
            }, (members, experience));
        }

        public void OnPartyMemberGainedExperience(PlayerManager player, uint experience)
        {
            if (partyMembersUIPools.TryFindObject(player.GetInstanceID(), out var partyMember))
                partyMember.AddGainedExperience(experience);
        }
        
        private void SetBonus(byte bonus)
        {
            if (bonus == 0)
            {
                bonusPanel.gameObject.SetActive(false);
                return;
            }

            bonusValueField.text = $"{bonus}%";
            bonusPanel.gameObject.SetActive(true);
        }

        private void AddMember(PlayerManager member)
        {
            var memberUI = partyMembersUIPools.GetObject();
            memberUI.Init(member, localPlayerIsLeader);
            memberUI.gameObject.transform.SetParent(partyMembersPanel, true);
            // Reset the scale manually back to 1 because it gets modified when the parent transform is changed for some reason
            memberUI.transform.localScale = Vector3.one;
        }
    }
}