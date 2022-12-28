using TMPro;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class GoldRewardUI : MonoBehaviour
    {
        public TextMeshProUGUI AmountField => amountField;
        [SerializeField] private TextMeshProUGUI amountField;
    }
}