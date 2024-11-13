using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Jupi.Components.DialogsManager;

/// <summary>
/// <see cref="UserControl"/> для отображения диалогового окна с вопросом
/// </summary>
public partial class QuestionMessageControl : UserControl
{
    /// <summary>
    /// Беспараметрический конструктор
    /// </summary>
    public QuestionMessageControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Действие по завершению загрузки
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;
        OkButton.Focus();
    }
}