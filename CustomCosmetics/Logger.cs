using BepInEx;
using BepInEx.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace CustomCosmetics;

internal class Logger
{
    private static ManualLogSource _logSource { get; set; }

    internal static void SetLogSource(ManualLogSource source)
    {
        _logSource = source;
    }

    public static void Message(object text, [CallerMemberName] string tag = "")
    {
        SendLog(text.ToString(), tag, LogLevel.Message);
    }

    public static void Warn(object text, [CallerMemberName] string tag = "")
    {
        SendLog(text.ToString(), tag, LogLevel.Warning);
    }

    public static void Error(object text, [CallerMemberName] string tag = "")
    {
        SendLog(text.ToString(), tag, LogLevel.Error);
    }

    public static void SendLog(string text, string tag = "", LogLevel logLevel = LogLevel.Info)
    {
        if (_logSource == null) return;

        var time = DateTime.Now.ToString("HH:mm:ss");
        var prefix = string.IsNullOrWhiteSpace(tag) ? "" : $" [{tag}]";
        var logMessage = $"[{time}]{prefix} {text}";

        switch (logLevel)
        {
            case LogLevel.Message: _logSource.LogMessage(logMessage); break;
            case LogLevel.Error: _logSource.LogError(logMessage); break;
            case LogLevel.Warning: _logSource.LogWarning(logMessage); break;
            default: _logSource.LogInfo(logMessage); break;
        }
    }
}