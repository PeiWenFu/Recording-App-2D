using System.Runtime.CompilerServices;
using UnityEngine;

public static class DebugLog
{
    public static void Info(string message, [CallerFilePath] string filePath = "")
    {
        string className = GetClassName(filePath);
        Debug.Log($"[{className}] {message}");
    }

    public static void Warning(string message, [CallerFilePath] string filePath = "")
    {
        string className = GetClassName(filePath);
        Debug.LogWarning($"[{className}] {message}");
    }

    public static void Error(object message, [CallerFilePath] string filePath = "")
    {
        string className = GetClassName(filePath);
        Debug.LogError($"[{className}] {message}");
    }

    private static string GetClassName(string filePath)
    {
        string[] splitPath = filePath.Split('/');
        string fileName = splitPath[splitPath.Length - 1];
        return fileName.Replace(".cs", "");
    }
}
