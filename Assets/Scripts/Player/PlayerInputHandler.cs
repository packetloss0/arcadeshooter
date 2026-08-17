using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcadeShooter.Player
{
    // I don't like playerinput "send messages" release message was getting lost and left the first button stuck.
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private bool logInput = true; //Debug

        [SerializeField] private float navigateThreshold = 0.5f;
        [SerializeField] private float navigateRepeatDelay = 0.4f;
        [SerializeField] private float navigateRepeatRate = 0.15f;

        public Vector2 Movement { get; private set; }
        public bool FireHeld => _aHeld || _rightTriggerHeld;

        public event Action FirePressed;
        public event Action PreviousWeaponPressed;
        public event Action NextWeaponPressed;
        public event Action StartPressed;
        public event Action ShockwavePressed;

        public event Action<int> MenuNavigate;

        private InputAction _movementAction;
        private InputAction _aAction, _bAction, _xAction, _yAction;
        private InputAction _leftTriggerAction, _rightTriggerAction, _startAction;

        private bool _aHeld;
        private bool _rightTriggerHeld;
        private int _navDirection;
        private float _navTimer;

        private void Awake()
        {
            var actions = GetComponent<PlayerInput>().actions;
            _movementAction = actions["Movement"];
            _aAction = actions["A"];
            _bAction = actions["B"];
            _xAction = actions["X"];
            _yAction = actions["Y"];
            _leftTriggerAction = actions["LeftTrigger"];
            _rightTriggerAction = actions["RightTrigger"];
            _startAction = actions["Start"];
        }

        private void OnEnable()
        {
            _aAction.performed += OnAPerformed;
            _aAction.canceled += OnACanceled;
            _rightTriggerAction.performed += OnRightTriggerPerformed;
            _rightTriggerAction.canceled += OnRightTriggerCanceled;
            _bAction.performed += OnBPerformed;
            _xAction.performed += OnXPerformed;
            _yAction.performed += OnYPerformed;
            _leftTriggerAction.performed += OnLeftTriggerPerformed;
            _startAction.performed += OnStartPerformed;
        }

        private void OnDisable()
        {
            _aAction.performed -= OnAPerformed;
            _aAction.canceled -= OnACanceled;
            _rightTriggerAction.performed -= OnRightTriggerPerformed;
            _rightTriggerAction.canceled -= OnRightTriggerCanceled;
            _bAction.performed -= OnBPerformed;
            _xAction.performed -= OnXPerformed;
            _yAction.performed -= OnYPerformed;
            _leftTriggerAction.performed -= OnLeftTriggerPerformed;
            _startAction.performed -= OnStartPerformed;

            _aHeld = false;
            _rightTriggerHeld = false;
        }

        private void Update()
        {
            Vector2 move = _movementAction.ReadValue<Vector2>();
            if (move != Movement)
            {
                Movement = move;
                Log($"Movement = {Movement}");
            }

            UpdateMenuNavigation(move);
        }

        private void UpdateMenuNavigation(Vector2 move)
        {
            int direction = 0;
            if (move.y > navigateThreshold) direction = 1;
            else if (move.y < -navigateThreshold) direction = -1;

            if (direction == 0)
            {
                _navDirection = 0;
                return;
            }

            if (direction != _navDirection)
            {
                _navDirection = direction;
                _navTimer = navigateRepeatDelay;
                MenuNavigate?.Invoke(direction);
                return;
            }

            _navTimer -= Time.unscaledDeltaTime;
            if (_navTimer <= 0f)
            {
                _navTimer = navigateRepeatRate;
                MenuNavigate?.Invoke(direction);
            }
        }

        // I know I know... Sue me. 

        private void OnAPerformed(InputAction.CallbackContext ctx)
        {
            Log("A pressed");
            _aHeld = true;
            FirePressed?.Invoke();
        }

        private void OnACanceled(InputAction.CallbackContext ctx)
        {
            Log("A released");
            _aHeld = false;
        }

        private void OnRightTriggerPerformed(InputAction.CallbackContext ctx)
        {
            Log("RightTrigger pressed");
            _rightTriggerHeld = true;
            FirePressed?.Invoke();
        }

        private void OnRightTriggerCanceled(InputAction.CallbackContext ctx)
        {
            Log("RightTrigger released");
            _rightTriggerHeld = false;
        }

        private void OnBPerformed(InputAction.CallbackContext ctx)
        {
            Log("B pressed");
            ShockwavePressed?.Invoke();
        }

        private void OnXPerformed(InputAction.CallbackContext ctx)
        {
            Log("X pressed");
            PreviousWeaponPressed?.Invoke();
        }

        private void OnYPerformed(InputAction.CallbackContext ctx)
        {
            Log("Y pressed");
            NextWeaponPressed?.Invoke();
        }

        private void OnLeftTriggerPerformed(InputAction.CallbackContext ctx)
        {
            Log("LeftTrigger pressed");
        }

        private void OnStartPerformed(InputAction.CallbackContext ctx)
        {
            Log("Start pressed");
            StartPressed?.Invoke();
        }

        private void Log(string message)
        {
            if (logInput) Debug.Log($"[Input] {message}");
        }
    }
}
