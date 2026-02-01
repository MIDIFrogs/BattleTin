using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private string logFilePath;

    private void Awake()
    {
        logFilePath = Path.Combine(Application.persistentDataPath, "game.log");
        Application.logMessageReceived += HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine($"{System.DateTime.Now}: {type} - {logString}");
        }
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}
