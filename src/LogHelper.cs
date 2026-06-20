using BepInEx.Logging;

namespace CustomStyleAdder;

public static class LogHelper
{
    private static ManualLogSource _logger;
    public static void Init(ManualLogSource log) => _logger = log;

    public static void Info(object? obj)
    {
        _logger.LogInfo($"{obj}");
    }
    
    public static void Warn(object? obj)
    {
        _logger.LogWarning($"{obj}");
    }

    public static void Error(object? obj)
    {
        _logger.LogError($"{obj}");
    }

    public static void Debug(object? obj)
    {
        _logger.LogDebug($"{obj}");
    }
}