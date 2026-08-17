using ArcadeShooter.Core;
using ArcadeShooter.Enemies;
using ArcadeShooter.Player;
using ArcadeShooter.PowerUps;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcadeShooter.Spawning
{ 
    public class WaveManager : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float waveDuration = 45f;
        [SerializeField] private float baseSpawnInterval = 3f;
        [SerializeField] private float intermissionTime = 4f;
        [SerializeField] private List<EnemyGroupData> enemyGroups = new();
        [SerializeField] private AudioClip waveEndedCheerSfx;
        [SerializeField] private PowerUpData waveEndReward;

        [Header("Debug")]
        [SerializeField] private int debugStartWave = 0; // if >0 game starts on that wave
        [SerializeField] private float debugMinutesPerWave = 1f;

        [Header("Spawn placement")]
        [SerializeField] private SpawnWarning spawnWarningPrefab;
        [SerializeField] private Vector2 spawnAreaHalfExtents = new(18.5f, 10f);
        [SerializeField] private float minDistanceFromPlayer = 3.5f;
        [SerializeField] private float groupSpread = 2f;   // enemies of one group cluster together

        public int CurrentWave { get; private set; }
        public bool WaveActive { get; private set; }

        private Coroutine _waveRoutine;

        public void BeginWaves()
        {
            StopWaves();
            CurrentWave = 0;

            if (debugStartWave > 1)
            {
                CurrentWave = debugStartWave - 1;
                float minutes = CurrentWave * debugMinutesPerWave;
                DifficultyManager.Instance?.SkipAhead(minutes);

                Debug.Log($"[Debug] Starting on {debugStartWave} wave " +
                          $"(difficulty skipped {minutes} min)");
            }

            _waveRoutine = StartCoroutine(WaveLoop());
        }

        public void StopWaves()
        {
            if (_waveRoutine != null) StopCoroutine(_waveRoutine);
            _waveRoutine = null;
            WaveActive = false;

            // Clear the arena, including spawns that were still counting down
            foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
                Destroy(enemy.gameObject);
            foreach (var warning in FindObjectsByType<SpawnWarning>(FindObjectsSortMode.None))
                Destroy(warning.gameObject);
        }

        private IEnumerator WaveLoop()
        {
            while (true)
            {
                CurrentWave++;
                WaveActive = true;
                GameManager.Instance.NotifyWaveRunning();
                GameEvents.RaiseWaveStarted(CurrentWave);

                float waveTimer = 0f;
                while (waveTimer < waveDuration)
                {
                    SpawnRandomGroup();

                    float interval = baseSpawnInterval / (DifficultyManager.Instance?.SpawnRateMod ?? 1f);
                    yield return new WaitForSeconds(interval);
                    waveTimer += interval;
                }

                // Spawning is over. Wait for the arena to be cleared
                while (FindObjectsByType<Enemy>(FindObjectsSortMode.None).Length > 0 ||
                       FindObjectsByType<SpawnWarning>(FindObjectsSortMode.None).Length > 0)
                {
                    yield return new WaitForSeconds(0.5f);
                }

                WaveActive = false;
                GameEvents.RaiseWaveEnded(CurrentWave);
                
                AudioManager.Instance?.PlaySfx(waveEndedCheerSfx);
                GameEvents.RaiseAnnouncement($"Wave {CurrentWave} Finished!");

                if (waveEndReward != null)
                {
                    PowerUpSpawner.Instance?.Spawn(waveEndReward);
                }

                yield return new WaitForSeconds(intermissionTime);
            }
        }

        private void SpawnRandomGroup()
        {
            if (enemyGroups.Count == 0 || spawnWarningPrefab == null) return;

            int tier = DifficultyManager.Instance?.Tier ?? 0;
            var available = enemyGroups.Where(g => g.difficultyTier <= tier).ToList();
            if (available.Count == 0)
            {
                Debug.LogWarning($"No enemy groups available for difficulty tier {tier}");
                return;
            }

            var group = available[Random.Range(0, available.Count)];
            Vector3 groupCenter = PickPosition();

            StartCoroutine(SpawnGroup(group, groupCenter));
        }

        private IEnumerator SpawnGroup(EnemyGroupData group, Vector3 groupCenter)
        {
            foreach (var entry in group.enemies)
            {
                if (entry.enemy == null || entry.enemy.enemyPrefab == null) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    Vector3 position = ClampToArena(
                        groupCenter + (Vector3)(Random.insideUnitCircle * groupSpread));

                    var warning = Instantiate(spawnWarningPrefab, position, Quaternion.identity);
                    warning.Configure(entry.enemy.enemyPrefab, group.speedModifier);

                    yield return new WaitForSeconds(entry.enemy.spawnInterval / group.speedModifier);
                }
            }
        }

        private Vector3 PickPosition()
        {
            var player = Player.PlayerController.Local;
            Vector3 pos = Vector3.zero;

            for (int i = 0; i < 16; i++)
            {
                pos = new Vector3(
                    Random.Range(-spawnAreaHalfExtents.x, spawnAreaHalfExtents.x),
                    Random.Range(-spawnAreaHalfExtents.y, spawnAreaHalfExtents.y),
                    0f);

                if (player == null ||
                    Vector2.Distance(pos, player.transform.position) >= minDistanceFromPlayer)
                {
                    break;
                }
            }

            return pos;
        }

        private Vector3 ClampToArena(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, -spawnAreaHalfExtents.x, spawnAreaHalfExtents.x);
            pos.y = Mathf.Clamp(pos.y, -spawnAreaHalfExtents.y, spawnAreaHalfExtents.y);
            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, spawnAreaHalfExtents * 2f);
        }
    }
}
