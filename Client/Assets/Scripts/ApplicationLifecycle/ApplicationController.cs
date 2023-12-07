using Noobie.SanGuoSha.Infrastructure;
using System;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
#if !UNITY_EDITOR
using Microsoft.Extensions.Logging;
#endif
using Noobie.SanGuoSha.LocalEventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Logger = Microsoft.Extensions.Logging.Logger;
using Noobie.SanGuoSha.Actions;
using Noobie.SanGuoSha.GamePlay;
using Noobie.SanGuoSha.GamePlay.UI;
using Noobie.SanGuoSha.Games;
using Noobie.SanGuoSha.Lobby;
using Noobie.SanGuoSha.Settings;

namespace Noobie.SanGuoSha.ApplicationLifecycle
{
    internal class ApplicationController : LifetimeScope
    {
        [SerializeField]
        private UpdateRunner _updateRunner;
        [SerializeField]
        private AudioMixerConfigurator _audioMixerConfigurator;
        [SerializeField]
        private PopupManager _popupManager;

        [CanBeNull]
        private IDisposable _subscriptions;

        private LobbyServiceFacade _lobbyService;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(_updateRunner);
            builder.RegisterComponent(_audioMixerConfigurator);
            builder.RegisterComponent(_popupManager);

            MemoryPackFormatterProvider.Register(new MemoryPoolFormatter<byte>());

            builder.RegisterInstance(new MessageChannel<QuitApplicationMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<ClientDisconnectedMessage>()).AsImplementedInterfaces();
            builder.RegisterInstance(new MessageChannel<LobbyPacketReceivedMessage>()).AsImplementedInterfaces();
#if UNITY_EDITOR
            builder.RegisterInstance(new Logger()).AsImplementedInterfaces();
#else
            builder.RegisterInstance(new NullLogger()).AsImplementedInterfaces();
#endif
            builder.Register<LocalLobbyUser>(Lifetime.Singleton);
            builder.Register<LobbyHeartbeat>(Lifetime.Singleton);
            builder.Register<PacketsSender>(Lifetime.Singleton);
            builder.Register<PacketsReceiver>(Lifetime.Singleton);
            builder.Register<GameActionScheduler>(Lifetime.Transient);
            builder.Register<Game>(Lifetime.Scoped);
            builder.Register<GameSettingsManager>(Lifetime.Singleton);
            builder.RegisterEntryPoint<LobbyServiceFacade>().AsSelf();
        }

        private void Start()
        {
            Container.Resolve<GameSettingsManager>().Load();
            _lobbyService = Container.Resolve<LobbyServiceFacade>();

            var quitApplicationSub = Container.Resolve<ISubscriber<QuitApplicationMessage>>();
            var subHandles = new DisposableGroup();
            subHandles.Add(quitApplicationSub.Subscribe(QuitGame));
            _subscriptions = subHandles;

            Application.wantsToQuit += OnWantToQuit;

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_updateRunner.gameObject);
            DontDestroyOnLoad(_audioMixerConfigurator.gameObject);

            Application.targetFrameRate = 60;
            SceneManager.LoadScene("MainMenu");
        }

        protected override void OnDestroy()
        {
            _subscriptions?.Dispose();
            base.OnDestroy();
        }

        private bool OnWantToQuit()
        {
            _lobbyService?.Disconnect();
            return true;
        }

        private static void QuitGame(QuitApplicationMessage msg)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}
