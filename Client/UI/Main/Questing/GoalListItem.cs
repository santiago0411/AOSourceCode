using AOClient.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AOClient.UI.Main.Questing
{
    public class GoalListItem : MonoBehaviour, IPoolObject
    {
        public int InstanceId => GetInstanceID();
        public bool IsBeingUsed { get; set; }
        
        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI goalText;

        public void SetGoal(string text, float maxProgress)
        {
            goalText.text = text;
            slider.maxValue = maxProgress;
        }

        public void SetProgressSlider(float progress)
        {
            slider.value = progress;
        }

        public void ResetPoolObject()
        {
            goalText.text = string.Empty;
            slider.maxValue = 0f;
            slider.value = 0f;
            gameObject.SetActive(false);
            IsBeingUsed = false;
        }
    }
}