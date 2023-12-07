#nullable enable

using System;
using UnityEngine;

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        void LogInformation(string? message, params object?[] args);

        void LogWarning(string? message, params object?[] args);

        void LogDebug(string? message, params object?[] args);

        void LogError(string? message, params object?[] args);

        void LogError(Exception exception);
    }

    public class NullLogger : ILogger
    {
        public void LogInformation(string? message, params object?[] args) { }
        public void LogWarning(string? message, params object?[] args) { }
        public void LogDebug(string? message, params object?[] args) { }
        public void LogError(string? message, params object?[] args) { }
        public void LogError(Exception exception) { }
    }

    public class Logger : ILogger
    {
        public void LogInformation(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.LogFormat(message, args);
        }

        public void LogWarning(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.LogWarningFormat(message, args);
        }

        public void LogDebug(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.LogFormat(message, args);
        }

        public void LogError(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.LogErrorFormat(message, args);
        }

        public void LogError(Exception? exception)
        {
            if (exception == null)
            {
                return;
            }
            Debug.LogException(exception);
        }
    }
}
