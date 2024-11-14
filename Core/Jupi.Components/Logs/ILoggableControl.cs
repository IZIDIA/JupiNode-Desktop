namespace Jupi.Components.Logs;

/// <summary>
/// Интерфейс для оболочки, которая может выводить сообщения
/// </summary>
public interface ILoggableControl
{
    /// <summary>
    /// Вывод информационного сообщения в оболочку
    /// </summary>
    /// <param name="text"> Текст сообщения </param>
    void InfoToControl(string text);

    /// <summary>
    /// Вывод сообщения об ошибке в оболочку
    /// </summary>
    /// <param name="text"> Текст сообщения </param>
    void ErrorToControl(string text);

    /// <summary>
    /// Вывод предупреждения в оболочку
    /// </summary>
    /// <param name="text"> Текст сообщения </param>
    void WarnToControl(string text);
}