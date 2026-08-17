using System.Collections;
using UnityEngine;
using TMPro;
using ArcadeShooter.Core;

namespace ArcadeShooter.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text waveText;
        [SerializeField] private StatBar healthBar;
        [SerializeField] private CanvasGroup waveAnnouncement;
        [SerializeField] private Animator waveAnnouncementAnimator; // "Show" animation
        [SerializeField] private TMP_Text announcementText;         // power-up ticker
        [SerializeField] private float announcementDuration = 3.5f;
        [SerializeField] private StatBar shockwaveBar;

        private static readonly int ShowParam = Animator.StringToHash("Show");
        private Coroutine _announceRoutine;

        private void OnEnable()
        {
            GameEvents.OnScoreChanged += UpdateScore;
            GameEvents.OnHighScoreChanged += UpdateHighScore;
            GameEvents.OnPlayerHealthChanged += UpdateHealth;
            GameEvents.OnWaveStarted += ShowWave;
            GameEvents.OnAnnouncement += ShowAnnouncement;
            GameEvents.OnShockwaveChargeChanged += UpdateShockwave;
        }

        private void OnDisable()
        {
            GameEvents.OnAnnouncement -= ShowAnnouncement;
            GameEvents.OnShockwaveChargeChanged -= UpdateShockwave;
            GameEvents.OnScoreChanged -= UpdateScore;
            GameEvents.OnHighScoreChanged -= UpdateHighScore;
            GameEvents.OnPlayerHealthChanged -= UpdateHealth;
            GameEvents.OnWaveStarted -= ShowWave;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                UpdateHighScore(GameManager.Instance.HighScore);
        }

        private void UpdateScore(int score)
        {
            if (scoreText != null) scoreText.text = score.ToString("N0");
        }

        private void UpdateHighScore(int score)
        {
            if (highScoreText != null) highScoreText.text = $"HI {score:N0}";
        }

        private void UpdateHealth(int current, int max)
        {
            if (healthBar != null) healthBar.SetValue(current, max);
        }

        private void ShowWave(int wave)
        {
            if (waveText != null) waveText.text = $"WAVE {wave}";
            if (waveAnnouncementAnimator != null) waveAnnouncementAnimator.SetTrigger(ShowParam);
        }

        private void UpdateShockwave(int current, int required)
        {
            if (shockwaveBar != null) shockwaveBar.SetValue(current, required);
        }

        private void ShowAnnouncement(string message)
        {
            if (announcementText == null) return;

            announcementText.text = message;
            if (_announceRoutine != null) StopCoroutine(_announceRoutine);
            _announceRoutine = StartCoroutine(ClearAnnouncement());
        }

        private IEnumerator ClearAnnouncement()
        {
            yield return new WaitForSeconds(announcementDuration);
            announcementText.text = string.Empty;
            _announceRoutine = null;
        }
    }
}
