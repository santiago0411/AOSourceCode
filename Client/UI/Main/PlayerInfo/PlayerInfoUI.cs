using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.PlayerInfo
{
    public sealed class PlayerInfoUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerName, level, currentXp, maxXp, gold;
        [SerializeField] private Slider xpSlider;

        public void SetName(string name)
        {
            playerName.text = name;
        }

        public void SetGold(uint gold)
        {
            this.gold.text = gold.ToString();
        }

        public void AddCurrentXp(uint gainedXp)
        {
            uint.TryParse(this.currentXp.text, out uint currentXp);
            currentXp += gainedXp;
            xpSlider.value = currentXp;
            this.currentXp.text = $"{currentXp}";
        }

        public void SetLevelAndXp(string level, uint currentXp, uint maxXp)
        {
            this.level.text = level;

            xpSlider.maxValue = maxXp;
            this.maxXp.text = maxXp.ToString();

            xpSlider.value = currentXp;
            this.currentXp.text = currentXp.ToString();
        }
    }
}
