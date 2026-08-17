using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Interfaces;

namespace ArcadeShooter.Weapons
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject explosionPrefab; // optional AoE on hit (grenades etc.)

        private const float Skin = 0.02f;      // gap left after a bounce so we don't re-hit

        private Rigidbody2D _rb;
        private int _damage;
        private float _speed;
        private bool _ownedByPlayer;
        private float _spawnTime;
        private int _pierceRemaining;
        private int _bouncesRemaining;
        private float _radius;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.bodyType = RigidbodyType2D.Kinematic;

            var col = GetComponent<Collider2D>();
            _radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);
        }

        public void Initialize(int damage, float speed, bool ownedByPlayer, int pierce = 0, int bounces = 0)
        {
            _damage = damage;
            _ownedByPlayer = ownedByPlayer;
            _pierceRemaining = pierce;
            _bouncesRemaining = bounces;

            // Explosive bullets always detonate on first impact
            // piercing/bouncing modifiers don't apply
            if (explosionPrefab != null)
            {
                _pierceRemaining = 0;
                _bouncesRemaining = 0;
            }

            // Enemy projectiles scale with difficulty
            float mod = ownedByPlayer ? 1f : (DifficultyManager.Instance?.ProjectileSpeedMod ?? 1f);
            _speed = speed * mod;

            _rb.linearVelocity = transform.right * _speed;
            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (Time.time - _spawnTime > lifeTime)
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore friendly fire in both directions
            bool hitEnemy = other.CompareTag("Enemy");
            bool hitPlayer = other.CompareTag("Player");
            bool hitWall = other.gameObject.layer == LayerMask.NameToLayer("Walls");

            if (_ownedByPlayer && hitPlayer) return;
            if (!_ownedByPlayer && hitEnemy) return;
            if (!hitEnemy && !hitPlayer && !hitWall) return;

            if (hitEnemy || hitPlayer)
            {
                var damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null && damageable.IsAlive)
                {
                    damageable.TakeDamage(_damage);
                }

                // Piercing ammo: pass through enemies while charges remain
                if (_ownedByPlayer && hitEnemy && _pierceRemaining > 0)
                {
                    if (_pierceRemaining != int.MaxValue) _pierceRemaining--;
                    return;
                }

                // Bouncing bullets: ricochet off the enemy and keep flying
                if (_ownedByPlayer && hitEnemy && _bouncesRemaining > 0)
                {
                    Bounce(other);
                    return;
                }
            }

            // Bouncing bullets ricochet off walls too
            if (hitWall && _ownedByPlayer && _bouncesRemaining > 0)
            {
                Bounce(other);
                return;
            }

            if (hitEffectPrefab != null)
            {
                ObjectPool.Instance.Spawn(hitEffectPrefab, transform.position, Quaternion.identity);
            }

            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            if (destroyOnHit)
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        // I could've used unity physics bounceiness, but why wouldn't that work?
        // Currently bullets don't have a collider but a trigger, so that makes it so physics are disabled on bullets.
        // Why? Piercing bullets would not work anymore.
        private void Bounce(Collider2D surface)
        {
            Vector2 position = transform.position;
            Vector2 closest = surface.ClosestPoint(position);

            // Direction from the surface back to us is the normal to bounce off
            Vector2 normal = (position - closest).normalized;
            if (normal == Vector2.zero) normal = -_rb.linearVelocity.normalized;

            if (_bouncesRemaining != int.MaxValue) _bouncesRemaining--;

            Vector2 reflected = Vector2.Reflect(_rb.linearVelocity.normalized, normal);
            _rb.linearVelocity = reflected * _speed;
            transform.right = reflected;
            transform.position = closest + normal * (_radius + Skin);
        }

        public void OnSpawnedFromPool() { }
        public void OnReturnedToPool() => _rb.linearVelocity = Vector2.zero;
    }
}
