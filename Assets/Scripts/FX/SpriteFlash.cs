using System.Collections;
using UnityEngine;

namespace ArcadeShooter.FX
{
    public class SpriteFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] renderers;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color flashColor = Color.white;

        private Color[] _baseColors;
        private Coroutine _routine;

        private void Awake()
        {
            if (renderers == null || renderers.Length == 0)
                renderers = GetComponentsInChildren<SpriteRenderer>();

            CacheBaseColors();
        }

        private void OnDisable()
        {
            // Pooled or respawned objects should not come back while flashing
            _routine = null;
            SetFlash(0f);
        }

        public void CacheBaseColors()
        {
            _baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) _baseColors[i] = renderers[i].color;
            }
        }

        public void Flash()
        {
            if (!isActiveAndEnabled) return;
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            SetFlash(1f);

            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                SetFlash(1f - t / flashDuration);
                yield return null;
            }

            SetFlash(0f);
            _routine = null;
        }

        private void SetFlash(float amount)
        {
            if (_baseColors == null) return;

            for (int i = 0; i < renderers.Length && i < _baseColors.Length; i++)
            {
                var r = renderers[i];
                if (r == null) continue;

                Color color = Color.Lerp(_baseColors[i], flashColor, amount);
                color.a = _baseColors[i].a;   // keep the sprite transparency
                r.color = color;
            }
        }
    }
}
