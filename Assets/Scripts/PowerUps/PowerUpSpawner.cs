using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArcadeShooter.Core;

namespace ArcadeShooter.PowerUps
{
    public class PowerUpSpawner : MonoBehaviour
    {
        public static PowerUpSpawner Instance { get; private set; }

        [SerializeField] private PowerUpPickup pickupPrefab;
        [SerializeField] private List<PowerUpData> powerUps = new();
        [SerializeField] private float minInterval = 12f;
        [SerializeField] private float maxInterval = 22f;
        [SerializeField] private int maxActive = 3;
        [SerializeField] private Vector2 spawnAreaHalfExtents = new(18.5f, 10f);
        [SerializeField] private float minDistanceFromPlayer = 5f;
        [SerializeField] private AudioClip pickupSpawnedSFX;

        private readonly List<PowerUpPickup> _active = new();
        private Coroutine _routine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += StartSpawning;
            GameEvents.OnGameOver += StopSpawning;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= StartSpawning;
            GameEvents.OnGameOver -= StopSpawning;
        }

        private void StartSpawning()
        {
            StopSpawning();
            _routine = StartCoroutine(SpawnLoop());
        }

        private void StopSpawning()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = null;

            foreach (var pickup in _active)
            {
                if (pickup != null) Destroy(pickup.gameObject);
            }
            _active.Clear();
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));

                _active.RemoveAll(p => p == null);
                if (_active.Count >= maxActive || powerUps.Count == 0) continue;

                Spawn(powerUps[Random.Range(0, powerUps.Count)]);
            }
        }

        public PowerUpPickup Spawn(PowerUpData data) => Spawn(data, PickPosition()); // spawn specific pickup at random spot

        public PowerUpPickup Spawn(PowerUpData data, Vector3 position)
        {
            if (data == null || pickupPrefab == null) return null;

            var pickup = Instantiate(pickupPrefab, position, Quaternion.identity);
            AudioManager.Instance?.PlaySfx(pickupSpawnedSFX);
            pickup.Initialize(data);
            _active.Add(pickup);
            return pickup;
        }

        public PowerUpPickup Spawn(PowerUpType type)
        {
            var data = powerUps.Find(p => p != null && p.type == type);
            if (data == null)
            {
                Debug.LogWarning($"No PowerUpData of {type} in the spawner's list");
                return null;
            }
            return Spawn(data);
        }

        private Vector3 PickPosition()
        {
            var player = Player.PlayerController.Local;
            Vector3 pos = Vector3.zero;

            for (int i = 0; i < 12; i++)
            {
                pos = new Vector3(
                    Random.Range(-spawnAreaHalfExtents.x, spawnAreaHalfExtents.x),
                    Random.Range(-spawnAreaHalfExtents.y, spawnAreaHalfExtents.y),
                    0f);

                if (player == null ||
                    Vector2.Distance(pos, player.transform.position) >= minDistanceFromPlayer) // don't spawn on player.
                {
                    break;
                }
            }

            return pos;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, spawnAreaHalfExtents * 2f);
        }
    }
}
