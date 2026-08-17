using System;
using UnityEngine;
using TMPro;
using ArcadeShooter.Core;

namespace ArcadeShooter.UI
{
    public class MenuSelector : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] options;

        [Header("Styling")]
        [SerializeField] private Color normalColor = new(0.62f, 0.66f, 0.76f, 1f);
        [SerializeField] private Color selectedColor = new(1f, 0.85f, 0.35f, 1f);
        [SerializeField] private string selectedPrefix = "> ";
        [SerializeField] private float pulseSpeed = 4f;
        [SerializeField] private float pulseAmount = 0.12f;

        [Header("Audio")]
        [SerializeField] private AudioClip navigateSfx;
        [SerializeField] private AudioClip acceptSfx;
        [SerializeField] private float semitonesPerStep = -1f;
        [SerializeField] private bool pitchAcceptToo = true;

        public event Action<int> Accepted;

        public int Index { get; private set; }
        public int Count => options != null ? options.Length : 0;

        private string[] _labels;

        private void Awake()
        {
            CacheLabels();
        }

        private void OnEnable()
        {
            CacheLabels();
            Select(0);
        }

        private void CacheLabels()
        {
            if (options == null) return;
            if (_labels != null && _labels.Length == options.Length) return;

            _labels = new string[options.Length];
            for (int i = 0; i < options.Length; i++)
            {
                _labels[i] = options[i] != null ? options[i].text : string.Empty;
            }
        }

        private void Update()
        {
            if (options == null || Count == 0) return;
            var current = options[Index];
            if (current == null) return;

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount; // highlight
            current.transform.localScale = Vector3.one * pulse;
        }

        public void Select(int index)
        {
            if (Count == 0) return;

            Index = ((index % Count) + Count) % Count;   // wrap both ways
            Refresh();
        }

        public void Move(int direction)
        {
            if (Count == 0) return;

            int previous = Index;
            Select(Index - direction);

            if (Index != previous)
            {
                AudioManager.Instance?.PlaySfxPitched(navigateSfx, PitchFor(Index));
            }
        }

        public void Accept()
        {
            AudioManager.Instance?.PlaySfxPitched(
                acceptSfx, pitchAcceptToo ? PitchFor(Index) : 1f);

            Accepted?.Invoke(Index);
        }

        private float PitchFor(int index) =>
            Mathf.Pow(2f, index * semitonesPerStep / 12f);

        private void Refresh()
        {
            if (options == null) return;

            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] == null) continue;

                bool selected = i == Index;
                options[i].text = selected ? selectedPrefix + _labels[i] : _labels[i];
                options[i].color = selected ? selectedColor : normalColor;
                if (!selected) options[i].transform.localScale = Vector3.one;
            }
        }
    }
}
