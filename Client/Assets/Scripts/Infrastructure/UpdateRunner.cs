using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Noobie.SanGuoSha.Infrastructure
{
    public class UpdateRunner : MonoBehaviour
    {
        class SubscriberData
        {
            public float Period;
            public float NextCallTime;
            public float LastCallTime;
        }

        private readonly Queue<Action> _pendingHandlers = new();
        private readonly HashSet<Action<float>> _subscribers = new();
        private readonly Dictionary<Action<float>, SubscriberData> _subscriberData = new();

        private void OnDestroy()
        {
            _pendingHandlers.Clear();
            _subscribers.Clear();
            _subscriberData.Clear();
        }

        public void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null)
            {
                return;
            }

            if (onUpdate.Target == null)
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope and can't be unsubscribed from");
                return;
            }

            if (onUpdate.Method.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any())
            {
                Debug.LogError("Can't subscribe with an anonymous function that cannot be Unsubscribed, by checking for a character that can't exist in a declared method name.");
                return;
            }

            if (!_subscribers.Contains(onUpdate))
            {
                _pendingHandlers.Enqueue(() =>
                {
                    if (_subscribers.Add(onUpdate))
                    {
                        _subscriberData.Add(onUpdate, new SubscriberData { Period = updatePeriod, NextCallTime = 0, LastCallTime = Time.time });
                    }
                });
            }
        }

        public void Unsubscribe(Action<float> onUpdate)
        {
            _pendingHandlers.Enqueue(() =>
            {
                _subscribers.Remove(onUpdate);
                _subscriberData.Remove(onUpdate);
            });
        }

        // Update is called once per frame
        void Update()
        {
            while (_pendingHandlers.Count > 0)
            {
                _pendingHandlers.Dequeue()?.Invoke();
            }

            foreach (var subscriber in _subscribers)
            {
                var subscriberData = _subscriberData[subscriber];

                if (Time.time >= subscriberData.NextCallTime)
                {
                    subscriber.Invoke(Time.time - subscriberData.LastCallTime);
                    subscriberData.LastCallTime = Time.time;
                    subscriberData.NextCallTime = Time.time + subscriberData.Period;
                }
            }
        }

    }
}
