using System;

namespace GuildAcademy.Core.Audio
{
    /// <summary>
    /// Pure C# audio configuration and volume calculation logic.
    /// No Unity dependencies - testable in EditMode.
    /// </summary>
    public class AudioConfig
    {
        public const string PREFS_KEY_BGM_VOLUME = "BGMVolume";
        public const string PREFS_KEY_SE_VOLUME = "SEVolume";
        public const string PREFS_KEY_MASTER_VOLUME = "MasterVolume";

        public const float DEFAULT_VOLUME = 0.8f;
        public const float MIN_VOLUME = 0f;
        public const float MAX_VOLUME = 1f;
        public const float DEFAULT_FADE_DURATION = 1f;
        public const int INITIAL_SE_POOL_SIZE = 5;

        private float _bgmVolume;
        private float _seVolume;
        private float _masterVolume;
        private bool _isBgmMuted;
        private bool _isSeMuted;

        public float BGMVolume
        {
            get => _bgmVolume;
            set => _bgmVolume = ClampVolume(value);
        }

        public float SEVolume
        {
            get => _seVolume;
            set => _seVolume = ClampVolume(value);
        }

        public float MasterVolume
        {
            get => _masterVolume;
            set => _masterVolume = ClampVolume(value);
        }

        public bool IsBGMMuted
        {
            get => _isBgmMuted;
            set => _isBgmMuted = value;
        }

        public bool IsSEMuted
        {
            get => _isSeMuted;
            set => _isSeMuted = value;
        }

        public AudioConfig()
        {
            _bgmVolume = DEFAULT_VOLUME;
            _seVolume = DEFAULT_VOLUME;
            _masterVolume = DEFAULT_VOLUME;
            _isBgmMuted = false;
            _isSeMuted = false;
        }

        /// <summary>
        /// Clamp volume to [0, 1] range.
        /// </summary>
        public static float ClampVolume(float volume)
        {
            if (volume < MIN_VOLUME) return MIN_VOLUME;
            if (volume > MAX_VOLUME) return MAX_VOLUME;
            return volume;
        }

        /// <summary>
        /// Convert linear volume (0-1) to decibels for AudioMixer.
        /// Uses the formula: dB = Log10(volume) * 20
        /// Volume of 0 maps to -80dB (silence).
        /// </summary>
        public static float LinearToDecibels(float linearVolume)
        {
            linearVolume = ClampVolume(linearVolume);
            if (linearVolume <= 0.0001f) return -80f;
            return (float)Math.Log10(linearVolume) * 20f;
        }

        /// <summary>
        /// Get effective BGM volume (considers mute and master).
        /// </summary>
        public float GetEffectiveBGMVolume()
        {
            if (_isBgmMuted) return 0f;
            return _bgmVolume * _masterVolume;
        }

        /// <summary>
        /// Get effective SE volume (considers mute and master).
        /// </summary>
        public float GetEffectiveSEVolume()
        {
            if (_isSeMuted) return 0f;
            return _seVolume * _masterVolume;
        }

        /// <summary>
        /// Calculate interpolated volume for crossfade.
        /// </summary>
        /// <param name="t">Interpolation factor (0-1). 0 = start, 1 = end.</param>
        /// <param name="fadeOut">True for fade-out (1->0), false for fade-in (0->1).</param>
        /// <returns>Volume at time t.</returns>
        public static float CalculateFadeVolume(float t, bool fadeOut)
        {
            t = ClampVolume(t);
            return fadeOut ? 1f - t : t;
        }

        /// <summary>
        /// Check if a SE clip is considered duplicate (already playing).
        /// Returns true if the clip name already exists in the playing list.
        /// </summary>
        public static bool IsDuplicateSE(string clipName, string[] currentlyPlayingClipNames)
        {
            if (string.IsNullOrEmpty(clipName) || currentlyPlayingClipNames == null)
                return false;

            for (int i = 0; i < currentlyPlayingClipNames.Length; i++)
            {
                if (currentlyPlayingClipNames[i] == clipName)
                    return true;
            }
            return false;
        }
    }
}
