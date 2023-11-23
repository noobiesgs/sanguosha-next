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
using Noobie.SanGuoSha.Games;

namespace Noobie.SanGuoSha.ApplicationLifecycle
{
    internal class ApplicationController : LifetimeScope
    {
        [SerializeField]
        private UpdateRunner _updateRunner;

        [CanBeNull]
        private IDisposable _subscriptions;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            MemoryPackFormatterProvider.Register(new MemoryPoolFormatter<byte>());

            builder.RegisterInstance(new MessageChannel<QuitApplicationMessage>()).AsImplementedInterfaces();
#if UNITY_EDITOR
            builder.RegisterInstance(new Logger()).AsImplementedInterfaces();
#else
            builder.RegisterInstance(new NullLogger()).AsImplementedInterfaces();
#endif
            builder.Register<GameActionScheduler>(Lifetime.Transient);
            builder.Register<Game>(Lifetime.Scoped);
            builder.RegisterComponent(_updateRunner);
        }

        private void Start()
        {
            var quitApplicationSub = Container.Resolve<ISubscriber<QuitApplicationMessage>>();

            var subHandles = new DisposableGroup();
            subHandles.Add(quitApplicationSub.Subscribe(QuitGame));
            _subscriptions = subHandles;

            Application.wantsToQuit += OnWantToQuit;

            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_updateRunner.gameObject);
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
