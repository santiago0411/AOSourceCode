using UnityEngine;

namespace AO.Core
{
    public class Health : MonoBehaviour
    {
        /// <summary>Contains the maximum health.</summary>
        public int MaxHealth { get; private set; }
        /// <summary>Contains the current health.</summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// Initializes the object health component
        /// </summary>
        public void SetHealth(int maxHealth, int currentHealth)
        {
            MaxHealth = maxHealth;
            if (MaxHealth < 1) MaxHealth = 1;
            CurrentHealth = currentHealth;
        }

        /// <summary>
        /// Removes health from the object.
        /// </summary>
        /// <param name="damage">Amount of health to be removed.</param>
        /// <param name="dieCallback">Method to be called if the health goes below 0.</param>
        public void TakeDamage(int damage, System.Action dieCallback = null)
        {
            if (damage < 0) return;

            CurrentHealth -= damage;
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                dieCallback?.Invoke();
            }
        }

        /// <summary>
        /// Adds health if it isn't dead.
        /// </summary>
        /// <param name="amount">Amount of health to be added.</param>
        public void Heal(int amount)
        {
            if (amount < 0) return;

            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth)
            {
               CurrentHealth = MaxHealth;
            }
        }
    }
}
