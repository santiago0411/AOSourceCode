using TMPro;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class SkillRewardUI : MonoBehaviour
    {
        public TextMeshProUGUI SkillText => skillText;
        [SerializeField] private TextMeshProUGUI skillText;
    }
}