using System.Collections;
using UnityEngine;
using TMPro;
using ArcadeShooter.Core;

namespace ArcadeShooter.UI
{
    // simple "animation" for the run stats
    public class StatsReveal : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] blocks;
        [SerializeField] private float lineDelay = 0.06f;
        [SerializeField] private float startDelay = 0.25f;
        [SerializeField] private AudioClip lineSfx;

        private Coroutine _routine;

        public void Play()
        {
            if (!isActiveAndEnabled) return;

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(Reveal());
        }

        private void OnDisable()
        {
            _routine = null;
            ShowEverything();
        }

        private IEnumerator Reveal()
        {
            foreach (var block in blocks)
            {
                if (block == null) continue;
                block.ForceMeshUpdate();
                block.maxVisibleLines = 0;
            }

            yield return new WaitForSecondsRealtime(startDelay);

            foreach (var block in blocks)
            {
                if (block == null) continue;

                int lines = block.textInfo.lineCount;
                for (int i = 1; i <= lines; i++)
                {
                    block.maxVisibleLines = i;
                    AudioManager.Instance?.PlaySfxPitched(lineSfx, 1f, 0.35f);
                    yield return new WaitForSecondsRealtime(lineDelay);
                }
            }

            ShowEverything();
            _routine = null;
        }

        private void ShowEverything()
        {
            foreach (var block in blocks)
            {
                if (block != null) block.maxVisibleLines = int.MaxValue;
            }
        }
    }
}
