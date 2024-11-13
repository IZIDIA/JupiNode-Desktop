using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

namespace JupiNode_Desktop.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var task = ShowOpenFileDialogAsync(title: "Открыть проект 228");
        await task.WaitAsync(CancellationToken.None);
        var files = task.Result;
    }

    /// <summary>
    /// Открыть диалоговое окно выбора файла (Асинхронно)
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="allowMultiple"> Разрешить выбор нескольких файлов </param>
    /// <param name="patterns"> Тип выбираемых файлов - словарь: ключ - название типа файла, значение - список форматов </param>
    /// <param name="addPatternAllFiles"> Добавить паттерн для выбора всех файлов </param>
    /// <param name="startLocationPath"> Стартовое расположение проводника </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Коллекция выбранных файлов </returns>
    public static async Task<IReadOnlyList<IStorageFile>?> ShowOpenFileDialogAsync(
        string title = "Открытие",
        bool allowMultiple = false,
        IReadOnlyDictionary<string, List<string>>? patterns = null,
        bool addPatternAllFiles = false,
        string? startLocationPath = null,
        Window? ownerWindow = null)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is null)
        {
            return null;
        }

        var storage = ownerWindow is null ? desktop.MainWindow.StorageProvider : ownerWindow.StorageProvider;
        if (!storage.CanOpen)
        {
            //Log.Warning($"Невозможно открыть проводник на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
        };

        if (patterns is not null)
        {
            var fileTypes = patterns.Select(pattern => new FilePickerFileType(pattern.Key) { Patterns = pattern.Value })
                .ToList();
            if (addPatternAllFiles)
            {
                var fileType = new FilePickerFileType("Все файлы")
                {
                    Patterns = new List<string> { "*.*" }
                };
                fileTypes.Add(fileType);
            }

            options.FileTypeFilter = fileTypes;
        }

        if (startLocationPath is not null)
        {
            options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync((Uri)new Uri(startLocationPath));
        }

        var result = await storage.OpenFilePickerAsync(options);
        return result;
    }
}