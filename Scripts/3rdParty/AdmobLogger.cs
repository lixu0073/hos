using UnityEngine;

/// <summary>
/// AdMob广告专用日志记录器，提供格式化的广告相关日志输出功能。
/// 为AdMob广告模块提供统一的日志记录接口，支持不同级别的日志输出。
/// </summary>
public static class AdmobLogger
{
    static readonly string tag = "<color=purple>[ADMOB] </color>";
    static readonly System.Text.StringBuilder sb = new System.Text.StringBuilder();

    public static void Log(string message)
    {
        Debug.LogFormat(tag + message);
    }

    public static void LogFormat(string message)
    {
        Debug.LogFormat(tag + message);
    }

    public static void LogFormat(params string[] message)
    {
        if (sb.Length > 0)
            sb.Remove(0, sb.Length);

        for (int i = 0; i < message.Length; i++)
        {
            sb.Append(message[i]);
        }

        Debug.LogFormat(tag + sb.ToString());
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarningFormat(tag + message);
    }

    public static void LogError(string message)
    {
        Debug.LogErrorFormat(tag + message);
    }
}