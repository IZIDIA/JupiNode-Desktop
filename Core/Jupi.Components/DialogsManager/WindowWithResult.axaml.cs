using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using SukiUI.Controls;

namespace Jupi.Components.DialogsManager;

/// <summary>
/// Окно с возвращаемым результатом <see cref="JupiDialogResult"/>
/// </summary>
public partial class WindowWithResult : SukiWindow
{
    /// <summary>
    /// Беспараметрический конструктор
    /// </summary>
    public WindowWithResult()
    {
        InitializeComponent();
        Resources.TryGetResource("ButtonsPanelGridLength", ThemeVariant.Default, out var value);
        if (value is GridLength gridLength)
        {
            ButtonsPanelHeight = gridLength.Value;
        }
    }

    /// <summary>
    /// Высота панели с кнопками
    /// </summary>
    public double ButtonsPanelHeight { get; private set; }

    /// <summary>
    /// Нажатие на кнопку ОК
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    public void ButtonOk_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(1);
    }

    /// <summary>
    /// Нажатие на кнопку Да
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    public void ButtonYes_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(6);
    }

    /// <summary>
    /// Нажатие на кнопку Нет
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    public void ButtonNo_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(7);
    }

    /// <summary>
    /// Нажатие на кнопку Отмена
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    public void ButtonCancel_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(2);
    }

    /// <summary>
    /// Нажатие на кнопку Закрыть
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    private void ButtonClose_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(0);
    }
}