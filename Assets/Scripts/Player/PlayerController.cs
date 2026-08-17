using UnityEngine;
using ArcadeShooter.Core;

namespace ArcadeShooter.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Local { get; private set; }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float acceleration = 60f;

        [Header("Refs")]
        [SerializeField] private Transform aimPivot;       // weapon holder rotates around this
        [SerializeField] private Animator animator;        // run/idle blend
        [SerializeField] private SpriteRenderer bodySprite;

        public bool MovementLocked { get; set; }
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private Rigidbody2D _rb;
        private PlayerInputHandler _input;
        private PlayerPowerUps _powerUps;

        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        private void Awake()
        {
            Local = this;
            _rb = GetComponent<Rigidbody2D>();
            _input = GetComponent<PlayerInputHandler>();
            _powerUps = GetComponent<PlayerPowerUps>();

            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
        }

        private void OnEnable()
        {
            GameEvents.OnGameOver += LockMovement;
            GameEvents.OnGameStarted += UnlockMovement;
        }

        private void OnDisable()
        {
            GameEvents.OnGameOver -= LockMovement;
            GameEvents.OnGameStarted -= UnlockMovement;
        }

        private void FixedUpdate()
        {
            // Holding Fire button stops player in place. Joystick starts to aim instead of moving. 
            Vector2 wish = (MovementLocked || _input.FireHeld) ? Vector2.zero : _input.Movement;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            // Smooth acceleration toward wish velocity 
            float speedMultiplier = _powerUps != null ? _powerUps.MoveSpeedMultiplier : 1f;
            Vector2 targetVelocity = wish * (moveSpeed * speedMultiplier);
            _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }

        private void Update()
        {
            if (!MovementLocked && _input.Movement.sqrMagnitude > 0.1f)
            {
                AimDirection = _input.Movement.normalized;
            }

            if (aimPivot != null)
            {
                float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
                aimPivot.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (bodySprite != null && Mathf.Abs(AimDirection.x) > 0.01f)
            {
                bodySprite.flipX = AimDirection.x < 0f;
            }

            if (animator != null)
            {
                animator.SetFloat(SpeedParam, _rb.linearVelocity.magnitude);
            }
        }

        private void LockMovement() => MovementLocked = true;
        private void UnlockMovement()
        {
            MovementLocked = false;
            // Respawn at origin
            transform.position = Vector3.zero;
            _rb.linearVelocity = Vector2.zero;
        }
    }
}
