using System;

namespace ArcadeShooter.Core
{
    public static class GameEvents
    {
        public static event Action<int> OnScoreChanged;
        public static event Action<int> OnHighScoreChanged;
        public static event Action<int, int> OnPlayerHealthChanged;   // current, max
        public static event Action<int> OnWaveStarted;                // wave number
        public static event Action<int> OnWaveEnded;                  // wave number
        public static event Action OnPlayerDied;
        public static event Action OnGameOver;
        public static event Action OnGameStarted;
        public static event Action<UnityEngine.Vector3, int, string> OnEnemyKilled; // position, score, enemy name
        public static event Action<int> OnCoinCollected;             // score the coin awarded
        public static event Action OnPowerUpCollected;
        public static event Action OnShockwaveUsed;
        public static event Action<string> OnAnnouncement;                  // HUD ticker message
        public static event Action<int, int> OnShockwaveChargeChanged;      // current, required

        public static void RaiseScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void RaiseHighScoreChanged(int score) => OnHighScoreChanged?.Invoke(score);
        public static void RaisePlayerHealthChanged(int current, int max) => OnPlayerHealthChanged?.Invoke(current, max);
        public static void RaiseWaveStarted(int wave) => OnWaveStarted?.Invoke(wave);
        public static void RaiseWaveEnded(int wave) => OnWaveEnded?.Invoke(wave);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseGameStarted() => OnGameStarted?.Invoke();
        public static void RaiseEnemyKilled(UnityEngine.Vector3 pos, int score, string enemyName = "") => OnEnemyKilled?.Invoke(pos, score, enemyName);
        public static void RaiseCoinCollected(int score) => OnCoinCollected?.Invoke(score);
        public static void RaisePowerUpCollected() => OnPowerUpCollected?.Invoke();
        public static void RaiseShockwaveUsed() => OnShockwaveUsed?.Invoke();
        public static void RaiseAnnouncement(string message) => OnAnnouncement?.Invoke(message);
        public static void RaiseShockwaveChargeChanged(int current, int required) => OnShockwaveChargeChanged?.Invoke(current, required);
    }
}
