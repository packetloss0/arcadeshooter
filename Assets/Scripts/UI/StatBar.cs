using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArcadeShooter.UI
{
    public class StatBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fill;
        [SerializeField] private Image ghostFill;   // trails behind after damage
        [SerializeField] private TMP_Text label;

        [Header("Ghost")]
        [SerializeField] private float ghostDelay = 0.35f;
        [SerializeField] private float ghostDrainSpeed = 0.6f;

        [Header("Feedback")]
        [SerializeField] private float punchScale = 0.06f;
        [SerializeField] private float punchDecay = 8f;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private Color flashColor = Color.white;

        [Header("Fill color")]
        [SerializeField] private bool tintByAmount = true;
        [SerializeField] private Color fullColor = new(0.25f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color lowColor = new(0.95f, 0.25f, 0.25f, 1f);
        [SerializeField] private float lowThreshold = 0.35f; 

        [Header("Full state")]
        [SerializeField] private bool pulseWhenFull;
        [SerializeField] private Color fullPulseColor = new(1f, 1f, 1f, 1f);
        [SerializeField] private float pulseSpeed = 5f;

        [Header("Label")]
        [SerializeField] private string labelFormat = "{0}/{1}";
        [SerializeField] private string fullLabel = "";

        private float _value = 1f;
        private float _ghost = 1f;
        private float _ghostTimer;
        private float _punch;
        private float _flashTimer;
        private Vector3 _baseScale = Vector3.one;

        private void Awake()
        {
            _baseScale = transform.localScale;
            if (fill != null) fill.type = Image.Type.Filled;
            if (ghostFill != null) ghostFill.type = Image.Type.Filled;
        }

        public void SetValue(int current, int max)
        {
            current = Mathf.Max(current, 0);

            if (label != null)
            {
                bool full = max > 0 && current >= max;
                label.text = (full && !string.IsNullOrEmpty(fullLabel))
                    ? fullLabel
                    : string.Format(labelFormat, current, max);
            }

            SetNormalized(max > 0 ? (float)current / max : 0f);
        }

        public void SetNormalized(float normalized)
        {
            normalized = Mathf.Clamp01(normalized);

            if (normalized < _value - 0.0001f)
            {
                // hold the ghost valve at the old value so the loss is visible
                _ghostTimer = ghostDelay;
                _flashTimer = flashDuration;
                _punch = punchScale;
            }
            else if (normalized > _value + 0.0001f)
            {
                _ghost = normalized;      // gains fill both bars at once
                _punch = punchScale * 0.5f;
            }

            _value = normalized;
            if (_ghost < _value) _ghost = _value;

            ApplyFills();
        }

        private void Update()
        {
            if (_ghostTimer > 0f)
            {
                _ghostTimer -= Time.deltaTime;
            }
            else if (_ghost > _value)
            {
                _ghost = Mathf.Max(_value, _ghost - ghostDrainSpeed * Time.deltaTime);
            }

            if (_flashTimer > 0f) _flashTimer -= Time.deltaTime;

            if (_punch > 0.0001f)
            {
                _punch = Mathf.Lerp(_punch, 0f, punchDecay * Time.deltaTime);
                transform.localScale = _baseScale * (1f + _punch);
            }
            else if (transform.localScale != _baseScale)
            {
                transform.localScale = _baseScale;
            }

            ApplyFills();
        }

        private void ApplyFills()
        {
            if (fill != null)
            {
                fill.fillAmount = _value;
                fill.color = CurrentFillColor();
            }

            if (ghostFill != null)
            {
                ghostFill.fillAmount = _ghost;
                ghostFill.enabled = _ghost > _value + 0.0001f;
            }
        }

        private Color CurrentFillColor()
        {
            Color color = tintByAmount
                ? Color.Lerp(lowColor, fullColor, Mathf.InverseLerp(0f, Mathf.Max(lowThreshold, 0.01f), _value))
                : fullColor;

            if (pulseWhenFull && _value >= 1f)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
                color = Color.Lerp(color, fullPulseColor, pulse);
            }

            if (_flashTimer > 0f)
            {
                color = Color.Lerp(color, flashColor, _flashTimer / Mathf.Max(flashDuration, 0.01f));
            }

            return color;
        }
    }
}
