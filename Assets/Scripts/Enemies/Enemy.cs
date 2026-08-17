using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.Enemies.States;
using ArcadeShooter.Interfaces;
using ArcadeShooter.FX;
using ArcadeShooter.Player;
using ArcadeShooter.Weapons;

namespace ArcadeShooter.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private bool canAttackAtRange = false;   // ranged or melee
        [SerializeField] private GameObject enemyProjectilePrefab;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private GameObject deathParticlesPrefab;
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private SpriteFlash spriteFlash;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;               // optional walk/attack anims

        public EnemyData Data => data;
        public Rigidbody2D Body { get; private set; }
        public EnemyStateMachine StateMachine { get; } = new();
        public bool CanAttackAtRange => canAttackAtRange;
        public bool Invulnerable { get; set; }
        public float GroupSpeedModifier { get; set; } = 1f;

        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;

        private static readonly int MovingParam = Animator.StringToHash("Moving");

        private Color _baseColor = Color.white;

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            Body.gravityScale = 0f;
            Body.freezeRotation = true;

            if (spriteRenderer != null) _baseColor = spriteRenderer.color;
        }

        private void Start()
        {
            // Health scales with difficulty (round up so it never drops below data value)
            float healthMod = DifficultyManager.Instance?.EnemyHealthMod ?? 1f;
            CurrentHealth = Mathf.CeilToInt(data.health * healthMod);

            gameObject.tag = "Enemy";
            StateMachine.SetState(new SpawningState(this));
        }

        private void Update()
        {
            StateMachine.Tick(Time.deltaTime);
            if (animator != null) animator.SetBool(MovingParam, Body.linearVelocity.sqrMagnitude > 0.1f);
        }

        private void FixedUpdate()
        {
            StateMachine.FixedTick(Time.fixedDeltaTime);
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive || Invulnerable) return;

            CurrentHealth -= amount;
            if (spriteFlash != null) spriteFlash.Flash();

            if (CurrentHealth <= 0)
            {
                StateMachine.SetState(new DeadState(this));
            }
        }

        //Simple solution to enemies stacking up. Just push them away!
        public Vector2 ComputeSeparation()
        {
            Vector2 push = Vector2.zero;
            foreach (var col in Physics2D.OverlapCircleAll(transform.position, 1f))
            {
                if (col.gameObject == gameObject || !col.CompareTag("Enemy")) continue;

                Vector2 away = (Vector2)(transform.position - col.transform.position);
                float d = Mathf.Max(away.magnitude, 0.1f);
                push += away.normalized / d;
            }
            return push;
        }

        // Creeper blinker
        public void SetBlink(bool on)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = on ? Color.white : _baseColor;
            }
        }

        public void FaceDirection(Vector2 direction)
        {
            if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
            {
                spriteRenderer.flipX = direction.x < 0f;
            }
        }

        public void PerformRangedAttack(Vector2 direction)
        {
            AudioManager.Instance?.PlaySfxAt(shootSound, transform.position);
            if (enemyProjectilePrefab == null) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var go = ObjectPool.Instance.Spawn(enemyProjectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            if (go.TryGetComponent<Projectile>(out var projectile))
            {
                projectile.Initialize(data.contactDamage, 8f, ownedByPlayer: false);
            }
        }

        public void PlayDeathEffects(bool showScorePopup = true)
        {
            AudioManager.Instance?.PlaySfxAt(data.deathSound, transform.position);
            CameraShake.Instance?.Shake(Random.Range(0.05f, 0.15f), 0.15f);

            if (deathParticlesPrefab != null)
                Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

            if (showScorePopup && scorePopupPrefab != null)
            {
                var popup = ObjectPool.Instance.Spawn(scorePopupPrefab, transform.position, Quaternion.identity);
                var scorePopup = popup.GetComponent<UI.ScorePopup>();
                if (scorePopup != null) scorePopup.Show(data.scoreValue, data.scorePopupColor);
            }
        }

        // We have social anxiety, damage on contact.
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!IsAlive) return;
            if (!collision.gameObject.CompareTag("Player")) return;

            var playerHealth = collision.gameObject.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(data.contactDamage);
        }
    }
}
