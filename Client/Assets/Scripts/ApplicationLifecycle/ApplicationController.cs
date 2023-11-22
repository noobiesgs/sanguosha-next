using Noobie.SanGuoSha.Infrastructure;
using System;
using JetBrains.Annotations;
using MemoryPack;
using MemoryPack.Formatters;
using Noobie.SanGuoSha.LocalEventBus;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

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
