using System;
using System.IO;
using UnityEngine;

public static class Logger
{
    static string currentLogFile;

    static Logger()
    {
        currentLogFile = $"UnityLog-{DateTime.Now.ToString("dd-MM-yy_hh-mm-ss")}";
        File.WriteAllText(currentLogFile, $"Starting logger for Unity Server at {DateTime.Now.ToString()}\n");
    }

    public static void Log(string text, LogLevel logLevel = LogLevel.Info)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            File.AppendAllText(currentLogFile, $"\n[{Enum.GetName(typeof(LogLevel), logLevel)}] ({DateTime.Now.ToString("hh:mm:ss.fff tt")}) {text}");
            switch (logLevel)
            {
                case LogLevel.Error:
                    Debug.LogError(text);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(text);
                    break;
                default:
                    Debug.Log(text);
                    break;
            }
        });        
    }

    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug
    }
}
