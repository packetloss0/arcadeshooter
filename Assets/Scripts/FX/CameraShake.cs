using UnityEngine;

namespace ArcadeShooter.FX
{
    // Global camera shake utility.
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [SerializeField] private float decaySpeed = 2f;
        [SerializeField] private float maxOffset = 0.5f;

        private float _intensity;
        private Vector3 _basePosition;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _basePosition = transform.localPosition;
        }

        public void Shake(float intensity, float duration = 0.2f)
        {
            _intensity = Mathf.Max(_intensity, intensity);
        }

        private void LateUpdate()
        {
            _intensity = Mathf.Clamp01(_intensity - decaySpeed * Time.deltaTime);

            Vector2 offset = Random.insideUnitCircle * _intensity * maxOffset;
            transform.localPosition = _basePosition + (Vector3)offset;
        }
    }
}
