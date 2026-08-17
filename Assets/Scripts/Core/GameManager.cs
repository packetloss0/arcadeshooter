using UnityEngine;
using ArcadeShooter.Spawning;

namespace ArcadeShooter.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        WaveIntermission,
        GameOver
    }

    [DefaultExecutionOrder(-100)] // Bandaid solution to some weird bugs.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private WaveManager waveManager;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int CurrentWave => waveManager != null ? waveManager.CurrentWave : 0;

        private const string HighScoreKey = "ArcadeShooter.HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnPlayerDied += HandlePlayerDied;
            GameEvents.OnWaveEnded += HandleWaveEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
            GameEvents.OnWaveEnded -= HandleWaveEnded;
        }

        public void StartGame()
        {
            if (State == GameState.Playing) return;

            Score = 0;
            GameEvents.RaiseScoreChanged(Score);
            SetState(GameState.Playing);
            GameEvents.RaiseGameStarted();
            waveManager.BeginWaves();
        }

        public void ReturnToMenu()
        {
            SetState(GameState.MainMenu);
            waveManager.StopWaves();   // otherwise the run keeps going behind the menu
        }

        private void SetState(GameState newState)
        {
            if (State == newState) return;
            State = newState;
        }

        public void AddScore(int amount)
        {
            if (State != GameState.Playing) return;

            Score += amount;
            GameEvents.RaiseScoreChanged(Score);

            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
                GameEvents.RaiseHighScoreChanged(HighScore);
            }
        }

        private void HandleEnemyKilled(Vector3 position, int scoreValue, string enemyName)
        {
            AddScore(scoreValue);
        }

        private void HandleWaveEnded(int wave)
        {
            if (State != GameState.Playing) return;
            SetState(GameState.WaveIntermission);
        }

        public void NotifyWaveRunning()
        {
            if (State == GameState.WaveIntermission)
                SetState(GameState.Playing);
        }

        private void HandlePlayerDied()
        {
            SetState(GameState.GameOver);
            waveManager.StopWaves();
            PlayerPrefs.Save();
            GameEvents.RaiseGameOver();
        }
    }
}
