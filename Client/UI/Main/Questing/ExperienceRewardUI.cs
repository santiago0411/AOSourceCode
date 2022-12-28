using TMPro;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class ExperienceRewardUI : MonoBehaviour
    {
        public TextMeshProUGUI ExperienceValueText => experienceValueText;
        [SerializeField] private TextMeshProUGUI experienceValueText;
    }
}