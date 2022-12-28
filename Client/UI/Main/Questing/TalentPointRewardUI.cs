using TMPro;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class TalentPointRewardUI : MonoBehaviour
    {
        public TextMeshProUGUI PointsText => pointsText;
        [SerializeField] private TextMeshProUGUI pointsText;
    }
}