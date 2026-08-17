using UnityEngine;
using TMPro;
using ArcadeShooter.Core;
using ArcadeShooter.Interfaces;

namespace ArcadeShooter.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class ScorePopup : MonoBehaviour, IPoolable
    {
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private float floatSpeed = 1.5f;
        [SerializeField] private Color defaultColor = new(1f, 0.9f, 0.3f, 1f);

        private TMP_Text _text;
        private float _spawnTime;

        private void Awake() => _text = GetComponent<TMP_Text>();

        public void Show(int score) => Show(score, defaultColor);

        public void Show(int score, Color color)
        {
            if (_text == null) _text = GetComponent<TMP_Text>();

            _text.text = $"+{score}";
            _text.color = color;
        }

        private void Update()
        {
            float t = (Time.time - _spawnTime) / lifetime;

            transform.position += Vector3.up * floatSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * scaleCurve.Evaluate(t);

            if (t >= 1f)
            {
                ObjectPool.Instance.Return(gameObject);
            }
        }

        public void OnSpawnedFromPool() => _spawnTime = Time.time;
        public void OnReturnedToPool() { }
    }
}
