using System.Collections;
using UnityEngine;
using ArcadeShooter.Core;

namespace ArcadeShooter.FX
{
    // classic juice
    public class HitStop : MonoBehaviour
    {
        public static HitStop Instance { get; private set; }

        [SerializeField] private float freezeDuration = 0.08f; // TODO: Make the hitstop adjustable through the call, so it can change depending on the source.

        private Coroutine _routine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += CancelFreeze;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= CancelFreeze;
            Time.timeScale = 1f;   // !!! never leave the game frozen
        }

        public void Freeze() => Freeze(freezeDuration);

        public void Freeze(float duration)
        {
            if (_routine != null) return;   // already frozen, don't stack
            if (PauseController.IsPaused) return;
            _routine = StartCoroutine(FreezeRoutine(duration));
        }

        private IEnumerator FreezeRoutine(float duration)
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
            _routine = null;
        }

        public void CancelFreeze()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;
            Time.timeScale = 1f;
        }
    }
}
