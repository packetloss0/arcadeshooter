using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ArcadeShooter.Core;

namespace ArcadeShooter.FX
{
    [RequireComponent(typeof(Volume))]
    public class ScreenEffects : MonoBehaviour
    {
        [Header("Damage vignette")]
        [SerializeField] private float vignettePeak = 0.45f;
        [SerializeField] private float vignetteFade = 2.5f;

        // TODO: Bloom effect increase on shockwave perhaps?

        private Vignette _vignette;

        private float _baseVignette;
        private int _lastHealth = -1;

        private void Awake()
        {
            var profile = GetComponent<Volume>().profile;
            profile.TryGet(out _vignette);

            if (_vignette != null) _baseVignette = _vignette.intensity.value;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;

            if (_vignette != null) _vignette.intensity.value = _baseVignette;
        }

        private void HandleHealthChanged(int current, int max)
        {
            // Only flash on lost health, no heals or resets
            if (_lastHealth >= 0 && current < _lastHealth && _vignette != null)
            {
                _vignette.intensity.value = _baseVignette + vignettePeak;
            }
            _lastHealth = current;
        }

        private void Update()
        {
            if (_vignette != null)
            {
                _vignette.intensity.value = Mathf.MoveTowards(
                    _vignette.intensity.value, _baseVignette, vignetteFade * Time.unscaledDeltaTime);
            }
        }
    }
}
