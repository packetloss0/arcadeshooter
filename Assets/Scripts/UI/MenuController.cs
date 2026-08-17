using System.Text;
using UnityEngine;
using TMPro;
using ArcadeShooter.Core;
using ArcadeShooter.Player;

namespace ArcadeShooter.UI
{
    public class MenuController : MonoBehaviour
    {
        private enum Screen { None, Main, Stats, GameOver, Paused }

        [SerializeField] private PlayerInputHandler input;

        [Header("Main menu")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private MenuSelector mainMenuSelector;   // TODO: Start / Stats / Quit

        [Header("Stats")]
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private TMP_Text statsText;

        [Header("Pause")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private MenuSelector pauseSelector;   // Resume / Retry / Give up

        [Header("Game over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private MenuSelector gameOverSelector;   //TODO: Retry / Quit to menu
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text runStatsLeft;
        [SerializeField] private TMP_Text runStatsRight;
        [SerializeField] private StatsReveal runStatsReveal;
        [SerializeField] private float gameOverLockTime = 1f; // Ignore inputs for a bit so accidental game input does not trigger the menu.

        private Screen _screen = Screen.None;
        private float _screenShownAt;

        private bool InputLocked =>
            _screen == Screen.GameOver && Time.unscaledTime - _screenShownAt < gameOverLockTime;

        private void OnEnable()
        {
            input.MenuNavigate += HandleNavigate;
            input.FirePressed += HandleAccept;
            input.StartPressed += HandleStart;
            input.ShockwavePressed += HandleBack;

            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnGameOver += HandleGameOver;

            if (mainMenuSelector != null) mainMenuSelector.Accepted += OnMainMenuAccepted;
            if (gameOverSelector != null) gameOverSelector.Accepted += OnGameOverAccepted;
            if (pauseSelector != null) pauseSelector.Accepted += OnPauseAccepted;
        }

        private void OnDisable()
        {
            input.MenuNavigate -= HandleNavigate;
            input.FirePressed -= HandleAccept;
            input.StartPressed -= HandleStart;
            input.ShockwavePressed -= HandleBack;

            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnGameOver -= HandleGameOver;

            if (mainMenuSelector != null) mainMenuSelector.Accepted -= OnMainMenuAccepted;
            if (gameOverSelector != null) gameOverSelector.Accepted -= OnGameOverAccepted;
            if (pauseSelector != null) pauseSelector.Accepted -= OnPauseAccepted;
        }

        private void Start()
        {
            ShowScreen(Screen.Main);
        }

        private void HandleNavigate(int direction)
        {
            if (InputLocked) return;

            switch (_screen)
            {
                case Screen.Main: mainMenuSelector?.Move(direction); break;
                case Screen.GameOver: gameOverSelector?.Move(direction); break;
                case Screen.Paused: pauseSelector?.Move(direction); break;
            }
        }

        // Start pauses during play, otherwise it just confirms
        private void HandleStart()
        {
            if (_screen == Screen.None)
            {
                PauseGame();
                return;
            }

            if (_screen == Screen.Paused)
            {
                ResumeGame();
                return;
            }

            HandleAccept();
        }

        private void HandleAccept()
        {
            if (InputLocked) return;

            switch (_screen)
            {
                case Screen.Main: mainMenuSelector?.Accept(); break;
                case Screen.GameOver: gameOverSelector?.Accept(); break;
                case Screen.Paused: pauseSelector?.Accept(); break;
                case Screen.Stats: ShowScreen(Screen.Main); break;
            }
        }

        private void HandleBack()
        {
            if (InputLocked) return;
            if (_screen == Screen.Stats) ShowScreen(Screen.Main);
            else if (_screen == Screen.Paused) ResumeGame();
        }

        private void OnPauseAccepted(int index)
        {
            switch (index)
            {
                case 0: ResumeGame(); break;
                case 1: StartGame(); break;          // retry
                case 2: ReturnToMainMenu(); break;   // give up
            }
        }

        private void PauseGame()
        {
            ShowScreen(Screen.Paused);
            PauseController.Instance?.Pause();
        }

        private void ResumeGame()
        {
            PauseController.Instance?.Resume();
            ShowScreen(Screen.None);
        }

        private void OnMainMenuAccepted(int index)
        {
            switch (index)
            {
                case 0: StartGame(); break;
                case 1: ShowScreen(Screen.Stats); break;
                case 2: QuitApplication(); break;
            }
        }

        private void OnGameOverAccepted(int index)
        {
            if (index == 0) StartGame();
            else ReturnToMainMenu();
        }

        private void StartGame()
        {
            PauseController.Instance?.ResumeInstant();

            // StartGame ignores us while a run is still playing, so go back
            // to menu state first. Cheeky way to make the retry from pause menu work.
            GameManager.Instance?.ReturnToMenu();
            ShowScreen(Screen.None);
            GameManager.Instance?.StartGame();
            AudioManager.Instance?.SpinUpMusic();
        }

        private void ReturnToMainMenu()
        {
            PauseController.Instance?.ResumeInstant();
            GameManager.Instance?.ReturnToMenu();
            AudioManager.Instance?.PlayMenuMusic();   // giving up raises no game over event
            ShowScreen(Screen.Main);
        }

        private static void QuitApplication()
        {
            PlayerStats.Save();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void HandleGameStarted() => ShowScreen(Screen.None);

        private void HandleGameOver()
        {
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"SCORE {GameManager.Instance.Score:N0}";

            BuildRunStats();
            ShowScreen(Screen.GameOver);
            runStatsReveal?.Play();   // after the panel is on, or there is nothing to measure
        }

        // Value column is placed with a pos tag, spaces would not line up
        // because the font is not monospaced
        private const string Col = "<pos=68%>";

        private void BuildRunStats()
        {
            if (runStatsLeft != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"SCORE FROM ENEMIES{Col}{RunStats.ScoreFromEnemies:N0}");
                sb.AppendLine($"SCORE FROM COINS{Col}{RunStats.ScoreFromCoins:N0}");
                sb.Append($"ENEMIES KILLED{Col}{RunStats.EnemiesKilled:N0}");

                foreach (var pair in RunStats.KillsByType)
                {
                    sb.AppendLine();
                    sb.Append($"   {pair.Key}{Col}{pair.Value:N0}");
                }

                runStatsLeft.text = sb.ToString();
            }

            if (runStatsRight != null)
            {
                string weapon = RunStats.BestWeapon(out int weaponKills);
                if (string.IsNullOrEmpty(weapon)) weapon = "-";

                int seconds = Mathf.RoundToInt(RunStats.LongestNoDamage);

                var sb = new StringBuilder();
                sb.AppendLine($"BEST WEAPON{Col}{weapon}");
                sb.AppendLine($"   its kills{Col}{weaponKills:N0}");
                sb.AppendLine($"COINS PICKED UP{Col}{RunStats.CoinsCollected:N0}");
                sb.AppendLine($"PICKUPS TAKEN{Col}{RunStats.PowerUpsCollected:N0}");
                sb.AppendLine($"SHOCKWAVES USED{Col}{RunStats.ShockwavesUsed:N0}");
                sb.AppendLine($"HEALTH HEALED{Col}{RunStats.HealthHealed:N0}");
                sb.Append($"LONGEST UNHURT{Col}{seconds / 60:0}:{seconds % 60:00}");

                runStatsRight.text = sb.ToString();
            }
        }

        private void ShowScreen(Screen screen)
        {
            _screen = screen;
            _screenShownAt = Time.unscaledTime;

            if (mainMenuPanel != null) mainMenuPanel.SetActive(screen == Screen.Main);
            if (statsPanel != null) statsPanel.SetActive(screen == Screen.Stats);
            if (gameOverPanel != null) gameOverPanel.SetActive(screen == Screen.GameOver);
            if (pausePanel != null) pausePanel.SetActive(screen == Screen.Paused);

            if (screen == Screen.Stats) RefreshStats();

            if (PlayerController.Local != null) // Freeze the player (buggy)
                PlayerController.Local.MovementLocked = screen != Screen.None;
        }

        private void RefreshStats()
        {
            if (statsText == null) return;

            int seconds = PlayerStats.SecondsPlayed;
            var sb = new StringBuilder();

            sb.AppendLine($"HIGH SCORE          {GameManager.Instance?.HighScore ?? 0:N0}");
            sb.AppendLine($"GAMES PLAYED        {PlayerStats.GamesPlayed:N0}");
            sb.AppendLine($"TIME WASTED         {seconds / 3600:00}:{seconds / 60 % 60:00}:{seconds % 60:00}");
            sb.AppendLine();
            sb.AppendLine($"ENEMIES KILLED      {PlayerStats.EnemiesKilled:N0}");

            foreach (var type in PlayerStats.KnownKillTypes())
            {
                sb.AppendLine($"   {type,-16} {PlayerStats.KillsOf(type):N0}");
            }

            sb.AppendLine();
            sb.AppendLine($"COINS COLLECTED     {PlayerStats.CoinsCollected:N0}");
            sb.AppendLine($"POWER-UPS USED     {PlayerStats.PowerUpsCollected:N0}");
            sb.AppendLine($"SHOCKWAVES    {PlayerStats.ShockwavesUsed:N0}");
            sb.AppendLine($"WAVES FINISHED       {PlayerStats.WavesCleared:N0}");
            sb.Append($"BEST WAVE           {PlayerStats.BestWave:N0}");

            statsText.text = sb.ToString();
        }
    }
}
