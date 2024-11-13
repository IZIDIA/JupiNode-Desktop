using Avalonia.Controls;
using Avalonia.Layout;

namespace Jupi.Components.DialogsManager;

/// <summary>
/// Кнопка для диалоговых окон
/// </summary>
/// <remarks> Позволяет добавить кастомную кнопку вниз нового окна </remarks>
public class DialogButton : Button
{
    /// <summary>
    /// Беспараметрический конструктор
    /// </summary>
    public DialogButton()
    {
        HorizontalAlignment = HorizontalAlignment.Right;
    }

    /// <summary>
    /// Результат кнопки
    /// </summary>
    /// <remarks> Кнопка не обязательно должна иметь результат </remarks>
    public JupiDialogResult? DialogResult { get; set; }

    /// <summary>
    /// Порядок кнопки
    /// </summary>
    /// <remarks> Порядок расстановки кнопок слева направо </remarks>
    public double Order { get; set; }

    /// <summary>
    /// Стиль
    /// </summary>
    protected override Type StyleKeyOverride { get; } = typeof(Button);
}