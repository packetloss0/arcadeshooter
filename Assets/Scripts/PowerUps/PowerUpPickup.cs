using ArcadeShooter.Core;
using ArcadeShooter.Player;
using UnityEngine;

namespace ArcadeShooter.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private PowerUpData data;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private float lifetime = 15f;
        [SerializeField] private float blinkTime = 4f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 3f;
        [SerializeField] private AudioClip powerUpPickupSfx;

        private float _spawnTime;
        private Vector3 _basePosition;

        public void Initialize(PowerUpData powerUp)
        {
            data = powerUp;
            ApplyVisual();
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
            _basePosition = transform.position;
            ApplyVisual();
        }

        private void ApplyVisual()
        {
            if (sprite != null && data != null) sprite.color = data.color;
        }

        private void Update()
        {
            float age = Time.time - _spawnTime;
            float remaining = lifetime - age;

            if (remaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = _basePosition + Vector3.up * (Mathf.Sin(age * bobSpeed) * bobAmplitude);

            if (sprite != null && remaining < blinkTime)
            {
                sprite.enabled = Mathf.PingPong(Time.time * 6f, 1f) > 0.4f; // Blink before despawn.
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var powerUps = other.GetComponentInParent<PlayerPowerUps>();
            if (powerUps == null || data == null) return;

            powerUps.Apply(data);
            AudioManager.Instance?.PlaySfx(powerUpPickupSfx);
            Destroy(gameObject);
        }
    }
}
