using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Noobie.SanGuoSha.Settings
{
    [Serializable]
    public class GameSettings
    {
        internal bool Changed { get; set; }

        public AudioMixerSettings AudioMixer { get; set; } = new();

        public ServerSettings Server { get; set; } = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeregisterSettingChangedEvents()
        {
            AudioMixer.PropertyChanged -= OnSettingsChanged;
            Server.PropertyChanged -= OnSettingsChanged;
        }

        public void RegisterSettingChangedEvents()
        {
            DeregisterSettingChangedEvents();

            AudioMixer.PropertyChanged += OnSettingsChanged;
            Server.PropertyChanged += OnSettingsChanged;
        }

        private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            Changed = true;
        }
    }

    public class ServerSettings : INotifyPropertyChanged
    {
        private string _ip
#if UNITY_EDITOR
            = "127.0.0.1"
#endif
            ;
        private int _port
#if UNITY_EDITOR
                = 9527
#endif
            ;

        private string _accountName;

        public string Ip
        {
            get => _ip;
            set => SetField(ref _ip, value);
        }

        public int Port
        {
            get => _port;
            set => SetField(ref _port, value);
        }

        public string AccountName
        {
            get => _accountName;
            set => SetField(ref _accountName, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }


    [Serializable]
    public class AudioMixerSettings : INotifyPropertyChanged
    {
        private float _heroVoiceVolume = 0.5f;
        private float _musicVolume = 0.4f;
        private float _effectVolume = 0.5f;
        private float _cardVoiceVolume = 0.5f;
        private float _masterVolume = 1f;

        public float MasterVolume
        {
            get => _masterVolume;
            set => SetField(ref _masterVolume, value);
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set => SetField(ref _musicVolume, value);
        }

        public float EffectVolume
        {
            get => _effectVolume;
            set => SetField(ref _effectVolume, value);
        }

        public float CardVoiceVolume
        {
            get => _cardVoiceVolume;
            set => SetField(ref _cardVoiceVolume, value);
        }

        public float HeroVoiceVolume
        {
            get => _heroVoiceVolume;
            set => SetField(ref _heroVoiceVolume, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
