using UnityEngine;
using ArcadeShooter.Enemies;

namespace ArcadeShooter.Spawning
{
    // Telegraphed enemy spawn. (using fancy words now)
    public class SpawnWarning : MonoBehaviour
    {
        [SerializeField] private float duration = 1.4f;
        [SerializeField] private Transform handPivot;     // rotates 360 degrees over duration
        [SerializeField] private Transform fill;          // scales 0 -> 1 over duration
        [SerializeField] private SpriteRenderer ring;

        [SerializeField] private Color StartColor;
        [SerializeField] private Color EndColor;

        private GameObject _enemyPrefab;
        private float _speedModifier = 1f;
        private float _elapsed;

        public void Configure(GameObject enemyPrefab, float groupSpeedModifier)
        {
            _enemyPrefab = enemyPrefab;
            _speedModifier = groupSpeedModifier;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / duration);

            if (handPivot != null) handPivot.localRotation = Quaternion.Euler(0f, 0f, -360f * t);
            if (fill != null) fill.localScale = Vector3.one * t;
            if (ring != null) ring.color = Color.Lerp(StartColor, EndColor, t);

            if (t >= 1f) Spawn();
        }

        private void Spawn()
        {
            if (_enemyPrefab != null)
            {
                var go = Instantiate(_enemyPrefab, transform.position, Quaternion.identity);
                foreach (var enemy in go.GetComponentsInChildren<Enemy>())
                {
                    enemy.GroupSpeedModifier = _speedModifier;
                }
            }

            Destroy(gameObject);
        }
    }
}
