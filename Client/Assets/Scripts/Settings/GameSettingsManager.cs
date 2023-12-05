#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Gley.AllPlatformsSave;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Noobie.SanGuoSha.Settings
{
    internal class GameSettingsManager
    {
        private readonly ILogger _logger;

        private const string SaveFileName = "settings";

        private CancellationTokenSource? _cancellationTokenSource;
        private GameSettings? _settings;

        public GameSettingsManager(ILogger logger)
        {
            _logger = logger;
        }

        public GameSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    throw new InvalidOperationException("Settings not loaded yet.");
                }
                return _settings;
            }
        }

        public void Load()
        {
            API.Load<GameSettings>(Path.Combine(Application.persistentDataPath, SaveFileName), DataWasLoaded, true);
        }

        public void Save()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            Save(_cancellationTokenSource = new CancellationTokenSource()).Forget();
        }

        private async UniTaskVoid Save(CancellationTokenSource cts)
        {
            var cancel = await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: cts.Token).SuppressCancellationThrow();
            if (cancel)
            {
                return;
            }

            if (Settings.Changed)
            {
                Settings.Changed = false;
                API.Save(Settings, Path.Combine(Application.persistentDataPath, SaveFileName), DataWasSaved, true);
            }
        }

        private void DataWasLoaded(GameSettings data, SaveResult result, string message)
        {
            if (result is SaveResult.EmptyData or SaveResult.Error)
            {
                _settings = new GameSettings();
                if (result is SaveResult.Error)
                {
                    _logger.LogError(message);
                }
            }

            if (result is SaveResult.Success)
            {
                _settings?.DeregisterSettingChangedEvents();
                _settings = data;
                _settings.RegisterSettingChangedEvents();
            }
        }

        private void DataWasSaved(SaveResult result, string message)
        {
            if (result == SaveResult.Error)
            {
                _logger.LogError(message);
            }
        }
    }
}
