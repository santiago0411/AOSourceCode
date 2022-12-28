using AOClient.Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AOClient.UI.Main
{
    public class SkillUI : MonoBehaviour
    {
        public Skill Skill { get; set; }
        public byte Value { get; set; }
        public Button SubButton => subButton;
        public Button AddButton => addButton;
        public TextMeshProUGUI SkillNameText => skillNameText;
        public TextMeshProUGUI SkillValueText => skillValueText;

        [SerializeField] private Button subButton, addButton;
        [SerializeField] private TextMeshProUGUI skillNameText, skillValueText;
    }
}
