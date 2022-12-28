using AOClient.Network;
using AOClient.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Talents
{
    public abstract class TalentNodeUIBase : MonoBehaviour
    {
        protected byte NotCommittedPoints { get; private set; }

        [SerializeField] protected ButtonExtension button;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private byte maxPoints;
        
        private byte currentPoints;
        private Color originalColor;
        private Image borderImage;

        protected abstract void SendCanSkillUpPacket();
        protected abstract void WriteSkillUpTalent(Packet packet);

        protected virtual void Start()
        {
            originalColor = button.colors.normalColor;
            pointsText.gameObject.SetActive(maxPoints > 1);
            button.onLeftClick.AddListener(OnButtonLeftClick);
            button.onRightClick.AddListener(OnButtonRightClick);
        }

        public void UpdateCurrentPoints(byte newPoints)
        {
            currentPoints = newPoints;
            pointsText.text = $"{currentPoints} / {maxPoints}";
            
            // Get the image here before this method will be ran before Start()
            borderImage ??= button.GetComponent<Image>();
            
            if (currentPoints >= maxPoints)
                borderImage.color = Color.cyan;
            else if (currentPoints > 0)
                borderImage.color = Color.green;
        }

        private void OnButtonLeftClick()
        {
            if (currentPoints == maxPoints || UIManager.GameUI.StatsWindow.AvailableTalentPoints == 0)
                return;

            SendCanSkillUpPacket();
        }

        private void OnButtonRightClick()
        {
            if (NotCommittedPoints <= 0)
                return;
            
            var statsWindow = UIManager.GameUI.StatsWindow; 
            statsWindow.AvailableTalentPoints++;
            
            if (--NotCommittedPoints == 0)
            {
                statsWindow.NodesToSendSkillUp.Remove(this);
                borderImage.color = originalColor;
            }
            pointsText.text = $"{currentPoints + NotCommittedPoints} / {maxPoints}";
        }

        public void OnCanSkillUpReturn(bool canSkillUp)
        {
            if (!canSkillUp || (NotCommittedPoints + currentPoints) >= maxPoints)
                return;
            
            var statsWindow = UIManager.GameUI.StatsWindow; 
            statsWindow.AvailableTalentPoints--;
            NotCommittedPoints++;
            pointsText.text = $"{currentPoints + NotCommittedPoints} / {maxPoints}";
            statsWindow.NodesToSendSkillUp.Add(this);
            
            borderImage.color = Color.yellow;
        }

        public void WriteSkillUpTalentToPacket(Packet packet)
        {
            WriteSkillUpTalent(packet);
            NotCommittedPoints = 0;
        }
    }
}