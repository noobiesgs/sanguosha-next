#nullable enable

using UnityEngine;

namespace Microsoft.Extensions.Logging
{
    public interface ILogger
    {
        void LogInformation(string? message, params object?[] args);
        void LogDebug(string? message, params object?[] args);
    }

    public class NullLogger : ILogger
    {
        public void LogInformation(string? message, params object?[] args) { }

        public void LogDebug(string? message, params object?[] args) { }
    }

    public class Logger : ILogger
    {
        public void LogInformation(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.Log(string.Format(message, args));
        }

        public void LogDebug(string? message, params object?[] args)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            Debug.Log(string.Format(message, args));
        }
    }
}
