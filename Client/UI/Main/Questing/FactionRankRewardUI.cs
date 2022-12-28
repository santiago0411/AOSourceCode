using TMPro;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class FactionRankRewardUI : MonoBehaviour
    {
        public TextMeshProUGUI RankTextField => rankTextField;
        [SerializeField] private TextMeshProUGUI rankTextField;
    }
}