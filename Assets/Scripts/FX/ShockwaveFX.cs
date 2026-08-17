using System.Collections.Generic;
using UnityEngine;
using ArcadeShooter.Enemies;

namespace ArcadeShooter.FX
{
    public class ShockwaveFX : MonoBehaviour
    {
        [SerializeField] private float expandSpeed = 35f;
        [SerializeField] private float maxRadius = 55f;
        [SerializeField] private int damage = 9999;
        [SerializeField] private SpriteRenderer ring;

        private float _radius;
        private readonly HashSet<Enemy> _hit = new();

        private void Start()
        {
            CameraShake.Instance?.Shake(0.8f, 0.5f);
        }

        private void Update()
        {
            _radius += expandSpeed * Time.deltaTime;

            // localScale = diameter
            transform.localScale = Vector3.one * (_radius * 2f);

            if (ring != null)
            {
                float fade = 1f - Mathf.Clamp01(_radius / maxRadius);
                var c = ring.color;
                c.a = 0.4f * fade + 0.05f;
                ring.color = c;
            }

            foreach (var enemy in Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None))
            {
                if (_hit.Contains(enemy) || !enemy.IsAlive || enemy.Invulnerable) continue;

                if (Vector2.Distance(enemy.transform.position, transform.position) <= _radius)
                {
                    _hit.Add(enemy);

                    bool isBomber = enemy.Data != null && enemy.Data.explodesNearPlayer;
                    enemy.TakeDamage(damage);

                    // Every bomber the wave sets off gets its own freeze
                    if (isBomber) HitStop.Instance?.Freeze();
                }
            }

            if (_radius >= maxRadius)
            {
                Destroy(gameObject);
            }
        }
    }
}
