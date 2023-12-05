using Noobie.SanGuoSha.Settings;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace Noobie.SanGuoSha.GamePlay
{
    public class AudioMixerConfigurator : MonoBehaviour
    {
        [SerializeField]
        private AudioMixer _mixer;

        [SerializeField]
        private string _mixerVarMainVolume = "MasterVolume";
        [SerializeField]
        private string _mixerVarMusicVolume = "MusicVolume";
        [SerializeField]
        private string _mixerVarEffectVolume = "EffectVolume";
        [SerializeField]
        private string _mixerVarCardVoiceVolume = "CardVoiceVolume";
        [SerializeField]
        private string _mixerVarHeroVoiceVolume = "HeroVoiceVolume";

        public const float MinimumVolume = 0.0001f;
        public const float MaximumVolume = 1f;

        [Inject]
        private GameSettingsManager _gameSettingsManager;

        /// <summary>
        /// The audio sliders use a value between 0.0001 and 1, but the mixer works in decibels -- by default, -80 to 0.
        /// To convert, we use log10(slider) multiplied by 20. Why 20? because log10(.0001)*20=-80, which is the
        /// bottom range for our mixer, meaning it's disabled.
        /// </summary>
        private const float VolumeLog10Multiplier = 20;

        private void Start()
        {
            Configure();
        }

        public void Configure()
        {
            _mixer.SetFloat(_mixerVarMainVolume, GetVolumeInDecibels(_gameSettingsManager.Settings.AudioMixer.MasterVolume));
            _mixer.SetFloat(_mixerVarMusicVolume, GetVolumeInDecibels(_gameSettingsManager.Settings.AudioMixer.MusicVolume));
            _mixer.SetFloat(_mixerVarEffectVolume, GetVolumeInDecibels(_gameSettingsManager.Settings.AudioMixer.EffectVolume));
            _mixer.SetFloat(_mixerVarCardVoiceVolume, GetVolumeInDecibels(_gameSettingsManager.Settings.AudioMixer.CardVoiceVolume));
            _mixer.SetFloat(_mixerVarHeroVoiceVolume, GetVolumeInDecibels(_gameSettingsManager.Settings.AudioMixer.HeroVoiceVolume));
        }

        private float GetVolumeInDecibels(float volume)
        {
            if (volume <= 0)
            {
                volume = MinimumVolume;
            }
            if (volume > MaximumVolume)
            {
                volume = MaximumVolume;
            }
            return Mathf.Log10(volume) * VolumeLog10Multiplier;
        }
    }
}
