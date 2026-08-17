using UnityEngine;
using ArcadeShooter.Core;

namespace ArcadeShooter.Spawning
{
    public class CoinDropper : MonoBehaviour
    {
        [SerializeField] private Coin coinPrefab;
        [SerializeField, Range(0f, 1f)] private float dropChance = 0.6f;

        private void OnEnable()
        {
            GameEvents.OnEnemyKilled += HandleEnemyKilled;
            GameEvents.OnGameOver += ClearCoins;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyKilled -= HandleEnemyKilled;
            GameEvents.OnGameOver -= ClearCoins;
        }

        private void HandleEnemyKilled(Vector3 position, int scoreValue, string enemyName)
        {
            if (coinPrefab == null || Random.value > dropChance) return;
            Instantiate(coinPrefab, position, Quaternion.identity);
        }

        private void ClearCoins()
        {
            foreach (var coin in FindObjectsByType<Coin>(FindObjectsSortMode.None))
            {
                Destroy(coin.gameObject);
            }
        }
    }
}
