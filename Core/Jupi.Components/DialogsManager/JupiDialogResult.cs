using System.ComponentModel;

namespace Jupi.Components.DialogsManager;

/// <summary>
/// Результат окна сообщения
/// </summary>
/// <remarks> За основу взято System.Windows.MessageBoxResult </remarks>
public enum JupiDialogResult
{
    /// <summary>
    /// Окно сообщения не возвращает результата
    /// </summary>
    [Description("")]
    None = 0,

    /// <summary>
    /// Значение результата окна сообщения OK
    /// </summary>
    [Description("ОК")]
    OK = 1,

    /// <summary>
    /// Значение результата окна сообщения — Отмена.
    /// </summary>
    [Description("Отмена")]
    Cancel = 2,

    /// <summary>
    /// Значение результата окна сообщения — Да.
    /// </summary>
    [Description("Да")]
    Yes = 6,

    /// <summary>
    /// Значение результата окна сообщения - Нет
    /// </summary>
    [Description("Нет")]
    No = 7,
}