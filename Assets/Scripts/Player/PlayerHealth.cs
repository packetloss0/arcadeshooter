using System.Collections;
using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Interfaces;
using ArcadeShooter.FX;

namespace ArcadeShooter.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private float invulnerabilityTime = 0.5f;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private SpriteFlash spriteFlash;

        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        private bool _invulnerable;

        private void OnEnable()
        {
            GameEvents.OnGameStarted += ResetHealth;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= ResetHealth;
        }

        private void Start()
        {
            ResetHealth();
        }

        private void ResetHealth()
        {
            CurrentHealth = maxHealth;
            GameEvents.RaisePlayerHealthChanged(CurrentHealth, maxHealth);
        }

        public void Heal(int amount)
        {
            if (!IsAlive) return;
            CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
            GameEvents.RaisePlayerHealthChanged(CurrentHealth, maxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || _invulnerable) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            CurrentHealth -= amount;
            GameEvents.RaisePlayerHealthChanged(CurrentHealth, maxHealth);

            AudioManager.Instance?.PlaySfx(hitSound);
            CameraShake.Instance?.Shake(0.35f, 0.25f);
            if (spriteFlash != null) spriteFlash.Flash();

            if (CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }

        private void Die()
        {
            AudioManager.Instance?.PlaySfx(deathSound);
            GameEvents.RaisePlayerDied();
        }

        private IEnumerator InvulnerabilityRoutine() // Let's not die in 1 sec. not fun
        {
            _invulnerable = true;
            yield return new WaitForSeconds(invulnerabilityTime);
            _invulnerable = false;
        }
    }
}
