using UnityEngine;
/// <summary>
/// ��װDebug��
/// </summary>
public static class Debug
{
    public static void Log(object content, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.Log(text);
#endif
    }

    public static void Log(object content, Object context, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.Log(text, context);
#endif
    }

    public static void LogWarning(object content, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.LogWarning(text);
#endif
    }

    public static void LogWarning(object content, Object context, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.LogWarning(text, context);
#endif
    }

    public static void LogError(object content, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.LogError(text);
#endif
    }

    public static void LogError(object content, Object context, string color = "#FFFFFF")
    {
#if UNITY_EDITOR
        string text = ("<color=" + color + ">" + content + "</color>");
        UnityEngine.Debug.LogError(text, context);
#endif
    }

    public static void DrawRay(Vector3 start, Vector3 direction)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.DrawRay(start, direction);
#endif
    }

    public static void DrawRay(Vector3 start, Vector3 direction, Color color)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.DrawRay(start, direction, color);
#endif
    }

    public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.DrawRay(start, direction, color, duration);
#endif
    }

    public static void DrawRay(Vector3 start, Vector3 direction, Color color, float duration, bool depthTest)
    {
#if UNITY_EDITOR
        UnityEngine.Debug.DrawRay(start, direction, color, duration, depthTest);
#endif
    }
}