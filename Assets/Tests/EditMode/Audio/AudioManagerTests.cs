using System;
using NUnit.Framework;
using GuildAcademy.Core.Audio;

namespace GuildAcademy.Tests.EditMode.Audio
{
    [TestFixture]
    public class AudioConfigTests
    {
        private AudioConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = new AudioConfig();
        }

        #region Default Values

        [Test]
        public void NewAudioConfig_HasDefaultVolumes()
        {
            Assert.AreEqual(AudioConfig.DEFAULT_VOLUME, _config.BGMVolume);
            Assert.AreEqual(AudioConfig.DEFAULT_VOLUME, _config.SEVolume);
            Assert.AreEqual(AudioConfig.DEFAULT_VOLUME, _config.MasterVolume);
        }

        [Test]
        public void NewAudioConfig_IsNotMuted()
        {
            Assert.IsFalse(_config.IsBGMMuted);
            Assert.IsFalse(_config.IsSEMuted);
        }

        [Test]
        public void DefaultVolume_Is_0_8()
        {
            Assert.AreEqual(0.8f, AudioConfig.DEFAULT_VOLUME);
        }

        #endregion

        #region Volume Clamping

        [Test]
        public void ClampVolume_WithinRange_ReturnsUnchanged()
        {
            Assert.AreEqual(0.5f, AudioConfig.ClampVolume(0.5f));
            Assert.AreEqual(0f, AudioConfig.ClampVolume(0f));
            Assert.AreEqual(1f, AudioConfig.ClampVolume(1f));
        }

        [Test]
        public void ClampVolume_BelowMin_ReturnsMin()
        {
            Assert.AreEqual(0f, AudioConfig.ClampVolume(-0.5f));
            Assert.AreEqual(0f, AudioConfig.ClampVolume(-100f));
        }

        [Test]
        public void ClampVolume_AboveMax_ReturnsMax()
        {
            Assert.AreEqual(1f, AudioConfig.ClampVolume(1.5f));
            Assert.AreEqual(1f, AudioConfig.ClampVolume(100f));
        }

        [Test]
        public void BGMVolume_SetAboveMax_ClampedTo1()
        {
            _config.BGMVolume = 2f;
            Assert.AreEqual(1f, _config.BGMVolume);
        }

        [Test]
        public void SEVolume_SetBelowMin_ClampedTo0()
        {
            _config.SEVolume = -1f;
            Assert.AreEqual(0f, _config.SEVolume);
        }

        [Test]
        public void MasterVolume_SetNormal_StoredCorrectly()
        {
            _config.MasterVolume = 0.6f;
            Assert.AreEqual(0.6f, _config.MasterVolume, 0.001f);
        }

        #endregion

        #region Linear to Decibels

        [Test]
        public void LinearToDecibels_FullVolume_Returns0dB()
        {
            Assert.AreEqual(0f, AudioConfig.LinearToDecibels(1f), 0.01f);
        }

        [Test]
        public void LinearToDecibels_HalfVolume_ReturnsNegative6dB()
        {
            // Log10(0.5) * 20 = -0.301 * 20 = -6.02
            Assert.AreEqual(-6.02f, AudioConfig.LinearToDecibels(0.5f), 0.1f);
        }

        [Test]
        public void LinearToDecibels_ZeroVolume_ReturnsMinus80dB()
        {
            Assert.AreEqual(-80f, AudioConfig.LinearToDecibels(0f));
        }

        [Test]
        public void LinearToDecibels_VerySmallVolume_ReturnsMinus80dB()
        {
            Assert.AreEqual(-80f, AudioConfig.LinearToDecibels(0.00001f));
        }

        [Test]
        public void LinearToDecibels_NegativeVolume_ReturnsMinus80dB()
        {
            // Negative is clamped to 0, which returns -80dB
            Assert.AreEqual(-80f, AudioConfig.LinearToDecibels(-0.5f));
        }

        [Test]
        public void LinearToDecibels_TenPercent_ReturnsNegative20dB()
        {
            // Log10(0.1) * 20 = -1 * 20 = -20
            Assert.AreEqual(-20f, AudioConfig.LinearToDecibels(0.1f), 0.01f);
        }

        #endregion

        #region Effective Volume

        [Test]
        public void GetEffectiveBGMVolume_NotMuted_ReturnsBGMTimesMaster()
        {
            _config.BGMVolume = 0.5f;
            _config.MasterVolume = 0.8f;
            Assert.AreEqual(0.4f, _config.GetEffectiveBGMVolume(), 0.001f);
        }

        [Test]
        public void GetEffectiveBGMVolume_Muted_ReturnsZero()
        {
            _config.BGMVolume = 0.5f;
            _config.MasterVolume = 0.8f;
            _config.IsBGMMuted = true;
            Assert.AreEqual(0f, _config.GetEffectiveBGMVolume());
        }

        [Test]
        public void GetEffectiveSEVolume_NotMuted_ReturnsSETimesMaster()
        {
            _config.SEVolume = 0.6f;
            _config.MasterVolume = 0.5f;
            Assert.AreEqual(0.3f, _config.GetEffectiveSEVolume(), 0.001f);
        }

        [Test]
        public void GetEffectiveSEVolume_Muted_ReturnsZero()
        {
            _config.SEVolume = 0.6f;
            _config.MasterVolume = 0.5f;
            _config.IsSEMuted = true;
            Assert.AreEqual(0f, _config.GetEffectiveSEVolume());
        }

        [Test]
        public void GetEffectiveBGMVolume_MasterZero_ReturnsZero()
        {
            _config.BGMVolume = 1f;
            _config.MasterVolume = 0f;
            Assert.AreEqual(0f, _config.GetEffectiveBGMVolume());
        }

        #endregion

        #region Fade Volume

        [Test]
        public void CalculateFadeVolume_FadeOut_AtStart_Returns1()
        {
            Assert.AreEqual(1f, AudioConfig.CalculateFadeVolume(0f, fadeOut: true));
        }

        [Test]
        public void CalculateFadeVolume_FadeOut_AtEnd_Returns0()
        {
            Assert.AreEqual(0f, AudioConfig.CalculateFadeVolume(1f, fadeOut: true));
        }

        [Test]
        public void CalculateFadeVolume_FadeOut_AtMiddle_Returns0_5()
        {
            Assert.AreEqual(0.5f, AudioConfig.CalculateFadeVolume(0.5f, fadeOut: true), 0.001f);
        }

        [Test]
        public void CalculateFadeVolume_FadeIn_AtStart_Returns0()
        {
            Assert.AreEqual(0f, AudioConfig.CalculateFadeVolume(0f, fadeOut: false));
        }

        [Test]
        public void CalculateFadeVolume_FadeIn_AtEnd_Returns1()
        {
            Assert.AreEqual(1f, AudioConfig.CalculateFadeVolume(1f, fadeOut: false));
        }

        [Test]
        public void CalculateFadeVolume_ClampsBelowZero()
        {
            // t < 0 is clamped to 0, so fadeOut=true -> 1-0=1
            Assert.AreEqual(1f, AudioConfig.CalculateFadeVolume(-0.5f, fadeOut: true));
        }

        [Test]
        public void CalculateFadeVolume_ClampsAboveOne()
        {
            // t > 1 is clamped to 1, so fadeOut=true -> 1-1=0
            Assert.AreEqual(0f, AudioConfig.CalculateFadeVolume(1.5f, fadeOut: true));
        }

        #endregion

        #region Duplicate SE Detection

        [Test]
        public void IsDuplicateSE_ClipInList_ReturnsTrue()
        {
            var playing = new[] { "sword_hit", "magic_cast", "footstep" };
            Assert.IsTrue(AudioConfig.IsDuplicateSE("magic_cast", playing));
        }

        [Test]
        public void IsDuplicateSE_ClipNotInList_ReturnsFalse()
        {
            var playing = new[] { "sword_hit", "magic_cast" };
            Assert.IsFalse(AudioConfig.IsDuplicateSE("explosion", playing));
        }

        [Test]
        public void IsDuplicateSE_EmptyList_ReturnsFalse()
        {
            Assert.IsFalse(AudioConfig.IsDuplicateSE("sword_hit", new string[0]));
        }

        [Test]
        public void IsDuplicateSE_NullList_ReturnsFalse()
        {
            Assert.IsFalse(AudioConfig.IsDuplicateSE("sword_hit", null));
        }

        [Test]
        public void IsDuplicateSE_NullClipName_ReturnsFalse()
        {
            var playing = new[] { "sword_hit" };
            Assert.IsFalse(AudioConfig.IsDuplicateSE(null, playing));
        }

        [Test]
        public void IsDuplicateSE_EmptyClipName_ReturnsFalse()
        {
            var playing = new[] { "sword_hit" };
            Assert.IsFalse(AudioConfig.IsDuplicateSE("", playing));
        }

        #endregion

        #region Constants

        [Test]
        public void PrefsKeys_AreCorrect()
        {
            Assert.AreEqual("BGMVolume", AudioConfig.PREFS_KEY_BGM_VOLUME);
            Assert.AreEqual("SEVolume", AudioConfig.PREFS_KEY_SE_VOLUME);
            Assert.AreEqual("MasterVolume", AudioConfig.PREFS_KEY_MASTER_VOLUME);
        }

        [Test]
        public void InitialSEPoolSize_Is5()
        {
            Assert.AreEqual(5, AudioConfig.INITIAL_SE_POOL_SIZE);
        }

        [Test]
        public void DefaultFadeDuration_Is1()
        {
            Assert.AreEqual(1f, AudioConfig.DEFAULT_FADE_DURATION);
        }

        #endregion

        #region Mute Toggle

        [Test]
        public void ToggleBGMMute_SwitchesState()
        {
            Assert.IsFalse(_config.IsBGMMuted);
            _config.IsBGMMuted = true;
            Assert.IsTrue(_config.IsBGMMuted);
            _config.IsBGMMuted = false;
            Assert.IsFalse(_config.IsBGMMuted);
        }

        [Test]
        public void ToggleSEMute_SwitchesState()
        {
            Assert.IsFalse(_config.IsSEMuted);
            _config.IsSEMuted = true;
            Assert.IsTrue(_config.IsSEMuted);
            _config.IsSEMuted = false;
            Assert.IsFalse(_config.IsSEMuted);
        }

        #endregion
    }
}
