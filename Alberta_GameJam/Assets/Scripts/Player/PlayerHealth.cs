using UnityEngine;

namespace Game.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;

        public System.Action<float, float> onHealthChanged; // (current, max)
        public System.Action onDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f) return;
            currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f) return;
            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            onHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void Die()
        {
            onDeath?.Invoke();
            // Placeholder: disable controls or respawn logic
            gameObject.SetActive(false);
        }
    }
}