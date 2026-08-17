using System.Collections;
using UnityEngine;
using ArcadeShooter.Core;
using ArcadeShooter.PowerUps;

namespace ArcadeShooter.Player
{
    public class PlayerPowerUps : MonoBehaviour
    {
        public static PlayerPowerUps Local { get; private set; }

        [SerializeField] private float maxPermanentFireRateMultiplier = 3f;
        [SerializeField] private int maxPermanentPierce = 3;
        [SerializeField] private float maxPermanentMoveSpeedMultiplier = 1.5f;
        [SerializeField] private int maxPermanentBounces = 2;

        [SerializeField] private AudioClip powerUpFinishedSFX;

        private float _permanentFireRate = 1f;
        private float _boostFireRate = 1f;
        private Coroutine _fireRateRoutine;

        private int _permanentPierce;
        private bool _pierceEverything;
        private Coroutine _pierceRoutine;

        private float _permanentMoveSpeed = 1f;
        private float _boostMoveSpeed = 1f;
        private Coroutine _moveSpeedRoutine;

        private int _permanentBounces;
        private bool _bounceEverything;
        private Coroutine _bounceRoutine;

        public float FireRateMultiplier => _permanentFireRate * _boostFireRate;
        public int PierceCount => _pierceEverything ? int.MaxValue : _permanentPierce; // How many enemies shots go though.
        public float MoveSpeedMultiplier => _permanentMoveSpeed * _boostMoveSpeed;
        public int BounceCount => _bounceEverything ? int.MaxValue : _permanentBounces; // How many times a bullet can bounce.

        private void Awake()
        {
            Local = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += ResetAll;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= ResetAll;
        }

        private void ResetAll()
        {
            if (_fireRateRoutine != null) StopCoroutine(_fireRateRoutine);
            if (_pierceRoutine != null) StopCoroutine(_pierceRoutine);
            if (_moveSpeedRoutine != null) StopCoroutine(_moveSpeedRoutine);
            if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
            _fireRateRoutine = null;
            _pierceRoutine = null;
            _moveSpeedRoutine = null;
            _bounceRoutine = null;
            _permanentFireRate = 1f;
            _boostFireRate = 1f;
            _permanentPierce = 0;
            _pierceEverything = false;
            _permanentMoveSpeed = 1f;
            _boostMoveSpeed = 1f;
            _permanentBounces = 0;
            _bounceEverything = false;
        }

        public void Apply(PowerUpData data)
        {
            Debug.Log($"[PowerUp] Picked up {data.displayName}");
            GameEvents.RaisePowerUpCollected();
            GameEvents.RaiseAnnouncement(string.IsNullOrEmpty(data.description)
                ? data.displayName.ToUpperInvariant()
                : $"{data.displayName.ToUpperInvariant()} - {data.description}");

            switch (data.type)
            {
                case PowerUpType.Heal:
                    var health = GetComponent<PlayerHealth>();
                    if (health != null) health.Heal(data.healAmount);
                    break;

                case PowerUpType.FireRate:
                    if (_fireRateRoutine != null) StopCoroutine(_fireRateRoutine);
                    _fireRateRoutine = StartCoroutine(FireRateBoost(data));
                    break;

                case PowerUpType.PiercingAmmo:
                    if (_pierceRoutine != null) StopCoroutine(_pierceRoutine);
                    _pierceRoutine = StartCoroutine(PierceBoost(data));
                    break;

                case PowerUpType.MoveSpeed:
                    if (_moveSpeedRoutine != null) StopCoroutine(_moveSpeedRoutine);
                    _moveSpeedRoutine = StartCoroutine(MoveSpeedBoost(data));
                    break;

                case PowerUpType.BouncingBullets:
                    if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
                    _bounceRoutine = StartCoroutine(BounceBoost(data));
                    break;
            }
        }

        // TODO Add some sound when powerup ends.

        private IEnumerator MoveSpeedBoost(PowerUpData data)
        {
            _boostMoveSpeed = data.boostMultiplier;
            yield return new WaitForSeconds(data.duration);
            _boostMoveSpeed = 1f;
            _permanentMoveSpeed = Mathf.Min(
                _permanentMoveSpeed * data.permanentAmount, maxPermanentMoveSpeedMultiplier);
            AudioManager.Instance?.PlaySfx(powerUpFinishedSFX);
            GameEvents.RaiseAnnouncement(
                $"{data.displayName.ToUpperInvariant()} finished!");
            _moveSpeedRoutine = null;
        }

        private IEnumerator BounceBoost(PowerUpData data)
        {
            _bounceEverything = true;
            yield return new WaitForSeconds(data.duration);
            _bounceEverything = false;
            _permanentBounces = Mathf.Min(
                _permanentBounces + Mathf.Max(1, Mathf.RoundToInt(data.permanentAmount)), maxPermanentBounces);
            AudioManager.Instance?.PlaySfx(powerUpFinishedSFX);
            GameEvents.RaiseAnnouncement(
                $"{data.displayName.ToUpperInvariant()} finished!");
            _bounceRoutine = null;
        }

        private IEnumerator FireRateBoost(PowerUpData data)
        {
            _boostFireRate = data.boostMultiplier;
            yield return new WaitForSeconds(data.duration);
            _boostFireRate = 1f;
            _permanentFireRate = Mathf.Min(
                _permanentFireRate * data.permanentAmount, maxPermanentFireRateMultiplier);
            AudioManager.Instance?.PlaySfx(powerUpFinishedSFX);
            GameEvents.RaiseAnnouncement(
                $"{data.displayName.ToUpperInvariant()} finished!");
            _fireRateRoutine = null;
        }

        private IEnumerator PierceBoost(PowerUpData data)
        {
            _pierceEverything = true;
            yield return new WaitForSeconds(data.duration);
            _pierceEverything = false;
            _permanentPierce = Mathf.Min(
                _permanentPierce + Mathf.Max(1, Mathf.RoundToInt(data.permanentAmount)), maxPermanentPierce);
            AudioManager.Instance?.PlaySfx(powerUpFinishedSFX);
            GameEvents.RaiseAnnouncement(
                $"{data.displayName.ToUpperInvariant()} finished!");
            _pierceRoutine = null;
        }
    }
}
