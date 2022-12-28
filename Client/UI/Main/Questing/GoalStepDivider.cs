using AOClient.Core;
using UnityEngine;

namespace AOClient.UI.Main.Questing
{
    public class GoalStepDivider : MonoBehaviour, IPoolObject
    {
        public int InstanceId => GetInstanceID();
        public bool IsBeingUsed { get; set; }

        public void ResetPoolObject()
        {
            gameObject.SetActive(false);
            IsBeingUsed = false;
        }
    }
}