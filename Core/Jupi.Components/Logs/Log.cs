namespace Jupi.Components.Logs;

/// <summary>
/// Логирование
/// </summary>
public static class Log
{
    /// <summary>
    /// Ссылка на компонент в который ведется лог
    /// </summary>
    public static readonly List<ILoggableControl> Controls = new();

    /// <summary>
    /// Логгер NLOG
    /// </summary>
    private static LogWrapper _logger = new();

    /// <summary>
    /// Папка с логами
    /// </summary>
    public static string Folder => _logger.LogFolder;

    /// <summary>
    /// Включить выключить подробное ведение логов
    /// </summary>
    public static bool DebugLogsEnabled { get; set; }

    /// <summary>
    /// Вывод сообщения об ошибке с информацией об исключении
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="ex"> Ошибка </param>
    public static void Error(string message, Exception? ex = null)
    {
        _logger.Error(message, ex);
        foreach (var control in Controls)
        {
            control.ErrorToControl(message);
        }
    }

    /// <summary>
    /// Вывод информационного сообщения
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="sendToControls"> Флаг визуального отображения сообщения для пользователя в листе сообщений </param>
    public static void Info(string message, bool sendToControls = true)
    {
        _logger.Info(message);
        if (sendToControls)
        {
            foreach (var control in Controls)
            {
                control.InfoToControl(message);
            }
        }
    }

    /// <summary>
    /// Вывод предупреждения
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="sendToControls"> Флаг визуального отображения сообщения для пользователя в листе сообщений </param>
    public static void Warning(string message, bool sendToControls = true)
    {
        _logger.Warn(message);
        if (sendToControls)
        {
            foreach (var control in Controls)
            {
                control.WarnToControl(message);
            }
        }
    }

    /// <summary>
    /// Вывод сообщения об отладке
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="ex"> Ошибка </param>
    public static void Debug(string message, Exception? ex = null)
    {
        if (DebugLogsEnabled)
        {
            _logger.Debug(message, ex);
#if DEBUG
            foreach (var control in Controls)
            {
                control.InfoToControl($"[DEBUG] {message}");
            }
#endif
        }
    }
}