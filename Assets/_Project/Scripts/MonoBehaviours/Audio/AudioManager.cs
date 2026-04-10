using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using GuildAcademy.Core.Audio;

namespace GuildAcademy.MonoBehaviours.Audio
{
    /// <summary>
    /// Singleton AudioManager with BGM crossfade, SE pooling, scene-based BGM,
    /// AudioMixer integration, and PlayerPrefs volume persistence.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton

        private static AudioManager _instance;
        public static AudioManager Instance => _instance;

        #endregion

        #region Inspector Fields

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer _audioMixer;
        [SerializeField] private AudioMixerGroup _bgmMixerGroup;
        [SerializeField] private AudioMixerGroup _seMixerGroup;

        [Header("BGM Settings")]
        [SerializeField] private float _defaultFadeDuration = AudioConfig.DEFAULT_FADE_DURATION;

        [Header("SE Settings")]
        [SerializeField] private int _initialSEPoolSize = AudioConfig.INITIAL_SE_POOL_SIZE;

        [Header("Scene BGM Mapping")]
        [SerializeField] private List<SceneBGMEntry> _sceneBGMEntries = new List<SceneBGMEntry>();

        #endregion

        #region Private Fields

        private AudioSource _bgmSourceA;
        private AudioSource _bgmSourceB;
        private AudioSource _activeBGMSource;
        private readonly List<AudioSource> _sePool = new List<AudioSource>();
        private AudioConfig _config;
        private Coroutine _crossFadeCoroutine;
        private AudioClip _currentBGMClip;

        #endregion

        #region Properties

        public bool IsBGMMuted => _config.IsBGMMuted;
        public bool IsSEMuted => _config.IsSEMuted;
        public float BGMVolume => _config.BGMVolume;
        public float SEVolume => _config.SEVolume;
        public float MasterVolume => _config.MasterVolume;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _config = new AudioConfig();
            InitializeAudioSources();
            InitializeSEPool();
            LoadVolumeSettings();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Initialization

        private void InitializeAudioSources()
        {
            _bgmSourceA = gameObject.AddComponent<AudioSource>();
            _bgmSourceB = gameObject.AddComponent<AudioSource>();

            ConfigureBGMSource(_bgmSourceA);
            ConfigureBGMSource(_bgmSourceB);

            _activeBGMSource = _bgmSourceA;
        }

        private void ConfigureBGMSource(AudioSource source)
        {
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            if (_bgmMixerGroup != null)
            {
                source.outputAudioMixerGroup = _bgmMixerGroup;
            }
        }

        private void InitializeSEPool()
        {
            for (int i = 0; i < _initialSEPoolSize; i++)
            {
                CreateSESource();
            }
        }

        private AudioSource CreateSESource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.loop = false;
            source.playOnAwake = false;
            if (_seMixerGroup != null)
            {
                source.outputAudioMixerGroup = _seMixerGroup;
            }
            _sePool.Add(source);
            return source;
        }

        #endregion

        #region BGM Control

        /// <summary>
        /// Play BGM with optional crossfade. If the same clip is already playing, do nothing.
        /// </summary>
        public void PlayBGM(AudioClip clip, float fadeDuration = -1f)
        {
            if (clip == null) return;
            if (_currentBGMClip == clip && _activeBGMSource != null && _activeBGMSource.isPlaying)
                return;

            if (fadeDuration < 0f) fadeDuration = _defaultFadeDuration;

            if (_currentBGMClip == null || fadeDuration <= 0f)
            {
                // No current BGM or instant switch
                StopAllBGMCoroutines();
                _activeBGMSource.Stop();
                _activeBGMSource.clip = clip;
                _activeBGMSource.volume = GetBGMSourceVolume();
                _activeBGMSource.Play();
                _currentBGMClip = clip;
            }
            else
            {
                CrossFade(clip, fadeDuration);
            }
        }

        /// <summary>
        /// Crossfade from current BGM to a new clip.
        /// </summary>
        public void CrossFade(AudioClip newClip, float fadeDuration = -1f)
        {
            if (newClip == null) return;
            if (fadeDuration < 0f) fadeDuration = _defaultFadeDuration;

            StopAllBGMCoroutines();
            _crossFadeCoroutine = StartCoroutine(CrossFadeCoroutine(newClip, fadeDuration));
        }

        /// <summary>
        /// Stop BGM with optional fade-out.
        /// </summary>
        public void StopBGM(float fadeDuration = 0f)
        {
            StopAllBGMCoroutines();

            if (fadeDuration <= 0f)
            {
                _bgmSourceA.Stop();
                _bgmSourceB.Stop();
                _currentBGMClip = null;
            }
            else
            {
                _crossFadeCoroutine = StartCoroutine(FadeOutCoroutine(_activeBGMSource, fadeDuration));
            }
        }

        /// <summary>
        /// Pause the currently playing BGM.
        /// </summary>
        public void PauseBGM()
        {
            if (_activeBGMSource != null && _activeBGMSource.isPlaying)
            {
                _activeBGMSource.Pause();
            }
        }

        /// <summary>
        /// Resume the paused BGM.
        /// </summary>
        public void ResumeBGM()
        {
            if (_activeBGMSource != null && !_activeBGMSource.isPlaying && _activeBGMSource.clip != null)
            {
                _activeBGMSource.UnPause();
            }
        }

        /// <summary>
        /// Mute/unmute BGM.
        /// </summary>
        public void MuteBGM()
        {
            _config.IsBGMMuted = !_config.IsBGMMuted;
            ApplyBGMVolume();
        }

        #endregion

        #region SE Control

        /// <summary>
        /// Play a sound effect. Skips if the same clip is already playing (duplicate prevention).
        /// </summary>
        public void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            if (_config.IsSEMuted) return;

            // Duplicate check
            if (IsSEPlaying(clip)) return;

            var source = GetAvailableSESource();
            source.clip = clip;
            source.volume = GetSESourceVolume();
            source.Play();
        }

        /// <summary>
        /// Stop all currently playing sound effects.
        /// </summary>
        public void StopAllSE()
        {
            foreach (var source in _sePool)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }
        }

        /// <summary>
        /// Mute/unmute SE.
        /// </summary>
        public void MuteSE()
        {
            _config.IsSEMuted = !_config.IsSEMuted;
            ApplySEVolume();
        }

        private bool IsSEPlaying(AudioClip clip)
        {
            foreach (var source in _sePool)
            {
                if (source != null && source.isPlaying && source.clip == clip)
                    return true;
            }
            return false;
        }

        private AudioSource GetAvailableSESource()
        {
            foreach (var source in _sePool)
            {
                if (source != null && !source.isPlaying)
                    return source;
            }
            // Pool exhausted, expand
            return CreateSESource();
        }

        #endregion

        #region Volume Control

        /// <summary>
        /// Set BGM volume (0-1) and persist to PlayerPrefs.
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            _config.BGMVolume = volume;
            ApplyBGMVolume();
            PlayerPrefs.SetFloat(AudioConfig.PREFS_KEY_BGM_VOLUME, _config.BGMVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Set SE volume (0-1) and persist to PlayerPrefs.
        /// </summary>
        public void SetSEVolume(float volume)
        {
            _config.SEVolume = volume;
            ApplySEVolume();
            PlayerPrefs.SetFloat(AudioConfig.PREFS_KEY_SE_VOLUME, _config.SEVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Set master volume (0-1) and persist to PlayerPrefs.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            _config.MasterVolume = volume;
            ApplyAllVolumes();
            PlayerPrefs.SetFloat(AudioConfig.PREFS_KEY_MASTER_VOLUME, _config.MasterVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            _config.BGMVolume = PlayerPrefs.GetFloat(
                AudioConfig.PREFS_KEY_BGM_VOLUME, AudioConfig.DEFAULT_VOLUME);
            _config.SEVolume = PlayerPrefs.GetFloat(
                AudioConfig.PREFS_KEY_SE_VOLUME, AudioConfig.DEFAULT_VOLUME);
            _config.MasterVolume = PlayerPrefs.GetFloat(
                AudioConfig.PREFS_KEY_MASTER_VOLUME, AudioConfig.DEFAULT_VOLUME);

            ApplyAllVolumes();
        }

        private void ApplyBGMVolume()
        {
            float effectiveVolume = _config.GetEffectiveBGMVolume();

            // Apply to AudioMixer if available
            if (_audioMixer != null)
            {
                _audioMixer.SetFloat(AudioConfig.PREFS_KEY_BGM_VOLUME,
                    AudioConfig.LinearToDecibels(effectiveVolume));
            }
            else
            {
                // Fallback: direct AudioSource volume control
                if (_bgmSourceA != null && _bgmSourceA.isPlaying)
                    _bgmSourceA.volume = effectiveVolume;
                if (_bgmSourceB != null && _bgmSourceB.isPlaying)
                    _bgmSourceB.volume = effectiveVolume;
            }
        }

        private void ApplySEVolume()
        {
            float effectiveVolume = _config.GetEffectiveSEVolume();

            if (_audioMixer != null)
            {
                _audioMixer.SetFloat(AudioConfig.PREFS_KEY_SE_VOLUME,
                    AudioConfig.LinearToDecibels(effectiveVolume));
            }
            else
            {
                // Fallback: direct AudioSource volume control
                foreach (var source in _sePool)
                {
                    if (source != null)
                        source.volume = effectiveVolume;
                }
            }
        }

        private void ApplyMasterVolume()
        {
            if (_audioMixer != null)
            {
                _audioMixer.SetFloat(AudioConfig.PREFS_KEY_MASTER_VOLUME,
                    AudioConfig.LinearToDecibels(_config.MasterVolume));
            }
        }

        private void ApplyAllVolumes()
        {
            ApplyMasterVolume();
            ApplyBGMVolume();
            ApplySEVolume();
        }

        /// <summary>
        /// Get the volume to apply to a BGM AudioSource (used when AudioMixer is not set).
        /// </summary>
        private float GetBGMSourceVolume()
        {
            if (_audioMixer != null) return 1f; // Mixer handles volume
            return _config.GetEffectiveBGMVolume();
        }

        /// <summary>
        /// Get the volume to apply to a SE AudioSource (used when AudioMixer is not set).
        /// </summary>
        private float GetSESourceVolume()
        {
            if (_audioMixer != null) return 1f; // Mixer handles volume
            return _config.GetEffectiveSEVolume();
        }

        #endregion

        #region Scene BGM

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var clip = GetBGMForScene(scene.name);
            if (clip != null)
            {
                PlayBGM(clip);
            }
        }

        private AudioClip GetBGMForScene(string sceneName)
        {
            if (_sceneBGMEntries == null) return null;

            foreach (var entry in _sceneBGMEntries)
            {
                if (entry != null && entry.SceneName == sceneName)
                    return entry.BGMClip;
            }
            return null;
        }

        #endregion

        #region Coroutines

        private IEnumerator CrossFadeCoroutine(AudioClip newClip, float duration)
        {
            var fadingOutSource = _activeBGMSource;
            var fadingInSource = (_activeBGMSource == _bgmSourceA) ? _bgmSourceB : _bgmSourceA;

            fadingInSource.clip = newClip;
            fadingInSource.volume = 0f;
            fadingInSource.Play();

            float startVolume = fadingOutSource.volume;
            float targetVolume = GetBGMSourceVolume();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                fadingOutSource.volume = Mathf.Lerp(startVolume, 0f, t);
                fadingInSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }

            fadingOutSource.Stop();
            fadingOutSource.clip = null;
            fadingOutSource.volume = 0f;

            fadingInSource.volume = targetVolume;

            _activeBGMSource = fadingInSource;
            _currentBGMClip = newClip;
            _crossFadeCoroutine = null;
        }

        private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                source.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
            _currentBGMClip = null;
            _crossFadeCoroutine = null;
        }

        private void StopAllBGMCoroutines()
        {
            if (_crossFadeCoroutine != null)
            {
                StopCoroutine(_crossFadeCoroutine);
                _crossFadeCoroutine = null;
            }
        }

        #endregion
    }

    /// <summary>
    /// Maps a scene name to a BGM AudioClip for automatic scene-based BGM switching.
    /// </summary>
    [Serializable]
    public class SceneBGMEntry
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private AudioClip _bgmClip;

        public string SceneName => _sceneName;
        public AudioClip BGMClip => _bgmClip;
    }
}
