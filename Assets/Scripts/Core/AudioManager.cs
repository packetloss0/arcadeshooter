using UnityEngine;

namespace ArcadeShooter.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private float musicFadeTime = 1f;

        [Header("Music pitch")]
        [SerializeField] private float slowDownPitch = 0.35f; // Pitch the track down when player dies / pause
        [SerializeField] private float slowDownDuration = 0.7f;
        [SerializeField] private float spinUpFromPitch = 0.45f; // Pitch up when starting a new run (basically like balatro)
        [SerializeField] private float spinUpDuration = 0.7f;

        private float _musicVolume = 1f;
        private Coroutine _pitchRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (musicSource != null) _musicVolume = musicSource.volume;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += PlayGameMusic;
            GameEvents.OnGameOver += SlowDownMusic;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= PlayGameMusic;
            GameEvents.OnGameOver -= SlowDownMusic;
        }

        private void Start()
        {
            WarmUpMusic();
            PlayMenuMusic();
        }

        // Fix a issue where gameplay music had a delay before it was loaded for the first time.
        private void WarmUpMusic()
        {
            if (menuMusic != null && menuMusic.loadState == AudioDataLoadState.Unloaded)
                menuMusic.LoadAudioData();

            if (gameMusic != null && gameMusic.loadState == AudioDataLoadState.Unloaded)
                gameMusic.LoadAudioData();
        }

        public void PlaySfx(AudioClip clip, float volume = 1f, float pitchVariance = 0.05f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            sfxSource.PlayOneShot(clip, volume);
        }

        // Play sound at specific pitch
        public void PlaySfxPitched(AudioClip clip, float pitch, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume);
        }

        public void PlaySfxAt(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource.PlayClipAtPoint(clip, position, volume);
        }

        public void SetMusicPitch(float pitch)
        {
            if (_pitchRoutine != null) { StopCoroutine(_pitchRoutine); _pitchRoutine = null; }
            if (musicSource != null) musicSource.pitch = pitch;
        }
        public void SlowDownMusic() => RampMusicPitch(slowDownPitch, slowDownDuration);

        public void SpinUpMusic()
        {
            SetMusicPitch(spinUpFromPitch);
            RampMusicPitch(1f, spinUpDuration);
        }

        private void RampMusicPitch(float target, float duration)
        {
            if (musicSource == null) return;
            if (_pitchRoutine != null) StopCoroutine(_pitchRoutine);
            _pitchRoutine = StartCoroutine(PitchRoutine(target, duration));
        }

        private System.Collections.IEnumerator PitchRoutine(float target, float duration)
        {
            float start = musicSource.pitch;

            for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
            {
                musicSource.pitch = Mathf.Lerp(start, target, t / duration);
                yield return null;
            }

            musicSource.pitch = target;
            _pitchRoutine = null;
        }

        public void PlayMenuMusic()
        {
            SetMusicPitch(1f);   // the menu always plays at normal speed
            CrossfadeTo(menuMusic, false);
        }

        public void PlayGameMusic() => CrossfadeTo(gameMusic, true);   // a new run starts the track over

        private Coroutine _fadeRoutine;

        // TODO: Crossfade needs more work.
        private void CrossfadeTo(AudioClip clip, bool restart)
        {
            if (musicSource == null || clip == null) return;
            if (!restart && musicSource.clip == clip) return;
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(clip));
        }

        private System.Collections.IEnumerator FadeRoutine(AudioClip next)
        {
            float startVol = musicSource.volume;
            for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / musicFadeTime);
                yield return null;
            }
            musicSource.volume = 0f;
            musicSource.clip = next;
            musicSource.loop = true;
            musicSource.Play();

            // Fix for the bug where interrupted fade left the music permamently quieter each time.
            for (float t = 0; t < musicFadeTime; t += Time.unscaledDeltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, _musicVolume, t / musicFadeTime);
                yield return null;
            }
            musicSource.volume = _musicVolume;
        }
    }
}
