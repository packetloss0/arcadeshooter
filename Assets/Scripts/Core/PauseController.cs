using System.Collections;
using UnityEngine;
using ArcadeShooter.FX;

namespace ArcadeShooter.Core
{
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }
        public static bool IsPaused { get; private set; }

        [SerializeField] private float slowDuration = 0.35f;
        [SerializeField] private float resumeDuration = 0.15f;
        [SerializeField] private bool pitchMusic = true;

        private Coroutine _routine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            IsPaused = false;
        }

        private void OnDisable()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            if (pitchMusic) AudioManager.Instance?.SetMusicPitch(1f);
        }

        public void Pause()
        {
            IsPaused = true;
            HitStop.Instance?.CancelFreeze();   // a freeze ending mid pause would undo it
            Ramp(0f, slowDuration);
        }

        public void Resume()
        {
            IsPaused = false;
            Ramp(1f, resumeDuration);
        }

        public void ResumeInstant()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;

            IsPaused = false;
            Time.timeScale = 1f;
            if (pitchMusic) AudioManager.Instance?.SetMusicPitch(1f);
        }

        private void Ramp(float target, float duration)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(RampRoutine(target, duration));
        }

        private IEnumerator RampRoutine(float target, float duration)
        {
            float start = Time.timeScale;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                Apply(Mathf.Lerp(start, target, t / duration));
                yield return null;
            }

            Apply(target);
            _routine = null;
        }

        private void Apply(float scale)
        {
            Time.timeScale = scale;
            if (pitchMusic) AudioManager.Instance?.SetMusicPitch(Mathf.Max(scale, 0.35f));
        }
    }
}
