using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.PlayerInfo
{
    public sealed class PlayerResourcesUI : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private TextMeshProUGUI maxHpField;
        [SerializeField] private TextMeshProUGUI currentHpField;
        [SerializeField] private Slider hpSlider;

        [Header("Mana")]
        [SerializeField] private TextMeshProUGUI maxManaField;
        [SerializeField] private TextMeshProUGUI currentManaField;
        [SerializeField] private Slider manaSlider;

        [Header("Stamina")]
        [SerializeField] private TextMeshProUGUI maxStaminaField;
        [SerializeField] private TextMeshProUGUI currentStaminaField;
        [SerializeField] private Slider staminaSlider;

        [Header("Hunger")]
        [SerializeField] private TextMeshProUGUI maxHungerField;
        [SerializeField] private TextMeshProUGUI currentHungerField;
        [SerializeField] private Slider hungerSlider;

        [Header("Thirst")]
        [SerializeField] private TextMeshProUGUI maxThirstField;
        [SerializeField] private TextMeshProUGUI currentThirstField;
        [SerializeField] private Slider thirstSlider;

        public void SetCurrentHpText(int currentHp)
        {
            currentHpField.text = currentHp.ToString();
            hpSlider.value = currentHp;
        }

        public void SetMaxHpText(int maxHp)
        {   
            hpSlider.maxValue = maxHp;
            maxHpField.text = maxHp.ToString();
        }

        public void SetCurrentManaText(ushort currentMana)
        {
            currentManaField.text = currentMana.ToString();
            manaSlider.value = currentMana;
        }

        public void SetMaxManaText(ushort maxMana)
        {
            manaSlider.maxValue = maxMana;
            maxManaField.text = maxMana.ToString();
        }

        public void SetCurrentStaminaText(ushort currentStamina)
        {
            currentStaminaField.text = currentStamina.ToString();
            staminaSlider.value = currentStamina;
        }

        public void SetMaxStaminaText(ushort maxStamina)
        {
            staminaSlider.maxValue = maxStamina;
            maxStaminaField.text = maxStamina.ToString();
        }

        public void SetCurrentHungerText(ushort currentHunger)
        {
            currentHungerField.text = currentHunger.ToString();
            hungerSlider.value = currentHunger;
        }

        public void SetMaxHungerText(ushort maxHunger)
        {
            hungerSlider.maxValue = maxHunger;
            maxHungerField.text = maxHunger.ToString();
        }

        public void SetCurrentThirstText(ushort currentThirst)
        {
            currentThirstField.text = currentThirst.ToString();
            thirstSlider.value = currentThirst;
        }

        public void SetMaxThirstText(ushort maxThirst)
        {
            thirstSlider.maxValue = maxThirst;
            maxThirstField.text = maxThirst.ToString();
        }
    }
}
