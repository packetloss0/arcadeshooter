using UnityEngine;
using ArcadeShooter.Player;

namespace ArcadeShooter.Core
{
    public class StatsTracker : MonoBehaviour
    {
        private float _runStartTime;
        private bool _timing;

        private WeaponHolder _weaponHolder;
        private int _lastHealth = -1;
        private float _lastDamageTime;

        private void OnEnable()
        {
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnWaveEnded += HandleWaveEnded;
            GameEvents.OnCoinCollected += HandleCoinCollected;
            GameEvents.OnPowerUpCollected += HandlePowerUpCollected;
            GameEvents.OnShockwaveUsed += HandleShockwaveUsed;
            GameEvents.OnPlayerHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnWaveEnded -= HandleWaveEnded;
            GameEvents.OnCoinCollected -= HandleCoinCollected;
            GameEvents.OnPowerUpCollected -= HandlePowerUpCollected;
            GameEvents.OnShockwaveUsed -= HandleShockwaveUsed;
            GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;

            BankPlayTime();
            PlayerStats.Save();
        }

        private void HandleGameStarted()
        {
            PlayerStats.GamesPlayed++;
            RunStats.Reset();

            _runStartTime = Time.time;
            _timing = true;
            _lastHealth = -1;
            _lastDamageTime = Time.time;
        }

        private void HandleGameOver()
        {
            BankNoDamageStreak();
            BankPlayTime();
            PlayerStats.Save();
        }

        private void HandleEnemyKilled(Vector3 position, int score, string enemyName)
        {
            PlayerStats.AddKill(enemyName);
            RunStats.AddKill(enemyName, CurrentWeaponName());
            RunStats.ScoreFromEnemies += score;
        }

        private void HandleWaveEnded(int wave)
        {
            PlayerStats.WavesCleared++;
            if (wave > PlayerStats.BestWave) PlayerStats.BestWave = wave;
        }

        private void HandleCoinCollected(int score)
        {
            PlayerStats.CoinsCollected++;
            RunStats.CoinsCollected++;
            RunStats.ScoreFromCoins += score;
        }

        private void HandlePowerUpCollected()
        {
            PlayerStats.PowerUpsCollected++;
            RunStats.PowerUpsCollected++;
        }

        private void HandleShockwaveUsed()
        {
            PlayerStats.ShockwavesUsed++;
            RunStats.ShockwavesUsed++;
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (_lastHealth >= 0)
            {
                if (current > _lastHealth)
                {
                    RunStats.HealthHealed += current - _lastHealth;
                }
                else if (current < _lastHealth)
                {
                    BankNoDamageStreak();
                    _lastDamageTime = Time.time;
                }
            }

            _lastHealth = current;
        }

        // Whatever gun was in hand gets the credit for the kill
        private string CurrentWeaponName()
        {
            if (_weaponHolder == null) _weaponHolder = FindFirstObjectByType<WeaponHolder>();
            if (_weaponHolder == null || _weaponHolder.CurrentWeapon == null) return "";

            var data = _weaponHolder.CurrentWeapon.Data;
            return data != null ? data.displayName : "";
        }

        private void BankNoDamageStreak()
        {
            float streak = Time.time - _lastDamageTime;
            if (streak > RunStats.LongestNoDamage) RunStats.LongestNoDamage = streak;
        }

        private void BankPlayTime()
        {
            if (!_timing) return;
            _timing = false;
            PlayerStats.SecondsPlayed += Mathf.RoundToInt(Time.time - _runStartTime);
        }
    }
}
