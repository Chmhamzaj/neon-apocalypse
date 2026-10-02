using System;
using UnityEngine;

namespace NeonApocalypse.Combat
{
    public class Health : MonoBehaviour
    {
        [Min(1)] public float maxHealth = 100f;
        public float Current { get; private set; }
        public bool IsDead => Current <= 0.01f;
        public event Action<float, float> Damaged;
        public event Action Died;

        private bool deathSent;

        private void Awake() => Current = maxHealth;

        public void Damage(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Clamp(Current - amount, 0f, maxHealth);
            Damaged?.Invoke(Current, maxHealth);
            if (Current <= 0f && !deathSent)
            {
                deathSent = true;
                Died?.Invoke();
            }
        }

        public void SetMaxHealth(float value, bool refill = true)
        {
            maxHealth = Mathf.Max(1f, value);
            if (refill) Current = maxHealth;
            else Current = Mathf.Min(Current, maxHealth);
            Damaged?.Invoke(Current, maxHealth);
        }

        public void ResetHealth()
        {
            deathSent = false;
            Current = maxHealth;
            Damaged?.Invoke(Current, maxHealth);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Current = Mathf.Min(maxHealth, Current + amount);
            Damaged?.Invoke(Current, maxHealth);
        }
    }
}
