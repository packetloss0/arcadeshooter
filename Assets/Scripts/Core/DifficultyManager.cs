using UnityEngine;

namespace ArcadeShooter.Core
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        //  x = minutes played, y = multiplier
        [Header("Scaling curves")]
        [SerializeField] private AnimationCurve spawnRateMultiplier = AnimationCurve.Linear(0, 1, 5, 3);
        [SerializeField] private AnimationCurve enemySpeedMultiplier = AnimationCurve.Linear(0, 1, 5, 2);
        [SerializeField] private AnimationCurve projectileSpeedMultiplier = AnimationCurve.Linear(0, 1, 5, 2);
        [SerializeField] private AnimationCurve enemyHealthMultiplier = AnimationCurve.Linear(0, 1, 5, 2);

        private float _runStartTime;
        private bool _running;
        public float MinutesPlayed => _running ? (Time.time - _runStartTime) / 60f : 0f;
        public float SpawnRateMod => spawnRateMultiplier.Evaluate(MinutesPlayed);
        public float EnemySpeedMod => enemySpeedMultiplier.Evaluate(MinutesPlayed);
        public float ProjectileSpeedMod => projectileSpeedMultiplier.Evaluate(MinutesPlayed);
        public float EnemyHealthMod => enemyHealthMultiplier.Evaluate(MinutesPlayed);

        // Difficulty tier, used to unlock harder enemy groups.
        public int Tier => Mathf.FloorToInt(MinutesPlayed); // one tier per minute

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += HandleGameStarted;
            GameEvents.OnGameOver += HandleGameOver;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= HandleGameStarted;
            GameEvents.OnGameOver -= HandleGameOver;
        }

        private void HandleGameStarted()
        {
            _runStartTime = Time.time;
            _running = true;
        }

        // Pretend the run already went for a while. Difficulty scales up with time. (debug)
        public void SkipAhead(float minutes)
        {
            _runStartTime -= minutes * 60f;
        }

        private void HandleGameOver() => _running = false;
    }
}
