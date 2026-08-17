using ArcadeShooter.Core;
using ArcadeShooter.Player;
using UnityEngine;

namespace ArcadeShooter.Spawning
{
    // Had more in mind for the coins but currently they just charge up the shockwave ability.
    [RequireComponent(typeof(Collider2D))]
    public class Coin : MonoBehaviour
    {
        [SerializeField] private int chargeValue = 1;
        [SerializeField] private int scoreValue = 150;
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private Color scorePopupColor;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private float blinkTime = 3f;
        [SerializeField] private float magnetRange = 3f;
        [SerializeField] private float magnetSpeed = 10f;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private AudioClip coinPickUpSFX;

        private float _spawnTime;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;
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

            if (sprite != null && remaining < blinkTime)
            {
                sprite.enabled = Mathf.PingPong(Time.time * 6f, 1f) > 0.4f;
            }

            var player = PlayerController.Local;
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance <= magnetRange)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position, player.transform.position, magnetSpeed * Time.deltaTime);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var ability = other.GetComponentInParent<ShockwaveAbility>();

            // Score is only awarded when shockwave ability is full. 
            bool meterFull = ability != null && ability.Ready;

            if (meterFull)
            {
                GameManager.Instance?.AddScore(scoreValue);
                ShowScorePopup();
            }
            else
            {
                ability?.AddCharge(chargeValue);
            }

            AudioManager.Instance?.PlaySfx(coinPickUpSFX);
            GameEvents.RaiseCoinCollected(meterFull ? scoreValue : 0);
            Destroy(gameObject);
        }

        private void ShowScorePopup()
        {
            if (scorePopupPrefab == null || ObjectPool.Instance == null) return;

            var popup = ObjectPool.Instance.Spawn(scorePopupPrefab, transform.position, Quaternion.identity);
            var scorePopup = popup.GetComponent<UI.ScorePopup>();
            if (scorePopup != null) scorePopup.Show(scoreValue, scorePopupColor);
        }
    }
}
