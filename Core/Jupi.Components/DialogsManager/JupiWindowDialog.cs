using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using DialogHostAvalonia;
using Jupi.Components.Heleprs;

namespace Jupi.Components.DialogsManager;

/// <summary>
/// Функционал диалоговых окон
/// </summary>
public static class JupiWindowDialog
{
    /// <summary>
    /// Идентификатор для главного диалогового сервиса
    /// </summary>
    public const string MainDialogHostString = "MainDialogHost";

    /// <summary>
    /// Заголовок окна по умолчанию
    /// </summary>
    public const string DefaultTitle = "Jupi";

    /// <summary>
    /// Значение высоты и ширины по умолчанию для нового окна
    /// </summary>
    private const double DefaultSize = 500d;

    /// <summary>
    /// Размер шрифта для автоматически генерируемых окон
    /// </summary>
    private const int DefaultFontSize = 12;

    /// <summary>
    /// Показать диалоговое окно (Асинхронно)
    /// </summary>
    /// <typeparam name="T"> Тип возвращаемого объекта </typeparam>
    /// <param name="dialogWindow"> Собственное окно </param>
    /// <returns> Задача, которую можно использовать для получения результата закрытия диалогового окна </returns>
    public static async Task<T?> ShowDialogAsync<T>(Window dialogWindow)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return default;
        }

        var result = await dialogWindow.ShowDialog<T>(desktop.MainWindow);
        return result;
    }

    /// <summary>
    /// Показать диалоговое окно
    /// </summary>
    /// <typeparam name="T"> Тип возвращаемого объекта </typeparam>
    /// <param name="dialogWindow"> Собственное окно </param>
    /// <returns> Задача, которую можно использовать для получения результата закрытия диалогового окна </returns>
    public static T? ShowDialog<T>(Window dialogWindow)
    {
        var task = ShowDialogAsync<T>(dialogWindow);
        return task.WaitOnDispatcherFrame();
    }

    /// <summary>
    /// Показать диалоговое окно (Асинхронно)
    /// </summary>
    /// <param name="userControl"> Контент окна </param>
    /// <param name="buttons"> Отображаемые кнопки </param>
    /// <param name="title"> Заголовок </param>
    /// <param name="canResize"> Изменение размера </param>
    /// <param name="width"> Ширина </param>
    /// <param name="height"> Высота </param>
    /// <param name="icon"> Иконка </param>
    /// <param name="windowStartupLocation"> Начально расположение окна </param>
    /// <param name="windowState"> Состояние окна </param>
    /// <param name="position"> Позиция окна, если включен ручной режим расположения </param>
    /// <param name="windowOpenedHandler"> Событие в момент открытия окна </param>
    /// <param name="windowClosingHandler"> Событие в момент начала процесса закрытия окна </param>
    /// <param name="owner"> Окно владелец </param>
    /// <param name="closeOnEscape"> Закрывать по нажатию на Escape </param>
    /// <returns> Задача </returns>
    public static async Task<JupiDialogResult> ShowDialogAsync(
        Control userControl,
        JupiButtonEnum? buttons = null,
        string title = DefaultTitle,
        bool canResize = true,
        double width = double.NaN,
        double height = double.NaN,
        WindowIcon? icon = null,
        WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
        WindowState windowState = WindowState.Normal,
        PixelPoint position = default,
        EventHandler? windowOpenedHandler = null,
        EventHandler<WindowClosingEventArgs>? windowClosingHandler = null,
        Window? owner = null,
        bool closeOnEscape = true)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return JupiDialogResult.None;
        }

        var dialogWindow = buttons is not null || GetDialogButtons(userControl).Any()
            ? CreateWindowWithResult(userControl, buttons, title, canResize, width, height, icon, desktop.MainWindow.Icon, windowStartupLocation, windowState, position, windowOpenedHandler, windowClosingHandler)
            : CreateWindow(userControl, title, canResize, width, height, icon, desktop.MainWindow.Icon, windowStartupLocation, windowState, position, windowOpenedHandler, windowClosingHandler);

        var ownerWindow = owner ?? desktop.MainWindow;
        dialogWindow.Opened += WindowOnOpened;
        if (closeOnEscape)
        {
            dialogWindow.KeyUp += DialogWindowOnKeyUp;
        }

        int result;
        try
        {
            result = await dialogWindow.ShowDialog<int>(ownerWindow);
        }
        finally
        {
            dialogWindow.Opened -= WindowOnOpened;
            if (closeOnEscape)
            {
                dialogWindow.KeyUp -= DialogWindowOnKeyUp;
            }
        }

        // Очистка VisualParent
        if (dialogWindow is WindowWithResult windowWithResult)
        {
            windowWithResult.contentPresenter.Content = null;
        }

        dialogWindow.Content = null;

        return result switch
        {
            1 => JupiDialogResult.OK,
            2 => JupiDialogResult.Cancel,
            6 => JupiDialogResult.Yes,
            7 => JupiDialogResult.No,
            _ => JupiDialogResult.None
        };
    }

    /// <summary>
    /// Показать диалоговое окно
    /// </summary>
    /// <param name="userControl"> Контент окна </param>
    /// <param name="buttons"> Отображаемые кнопки </param>
    /// <param name="title"> Заголовок </param>
    /// <param name="canResize"> Изменение размера </param>
    /// <param name="width"> Ширина </param>
    /// <param name="height"> Высота </param>
    /// <param name="icon"> Иконка </param>
    /// <param name="windowStartupLocation"> Начально расположение окна </param>
    /// <param name="windowState"> Состояние окна </param>
    /// <param name="position"> Позиция окна, если включен ручной режим расположения </param>
    /// <param name="windowOpenedHandler"> Событие в момент открытия окна </param>
    /// <param name="windowClosingHandler"> Событие в момент начала процесса закрытия окна </param>
    /// <param name="owner"> Окно владелец </param>
    /// <param name="closeOnEscape"> Закрывать по нажатию на Escape </param>
    /// <returns> Результат диалога </returns>
    public static JupiDialogResult ShowDialog(
        Control userControl,
        JupiButtonEnum? buttons = null,
        string title = DefaultTitle,
        bool canResize = true,
        double width = double.NaN,
        double height = double.NaN,
        WindowIcon? icon = null,
        WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
        WindowState windowState = WindowState.Normal,
        PixelPoint position = default,
        EventHandler? windowOpenedHandler = null,
        EventHandler<WindowClosingEventArgs>? windowClosingHandler = null,
        Window? owner = null,
        bool closeOnEscape = true)
    {
        var task = ShowDialogAsync(
            userControl: userControl,
            buttons: buttons,
            title: title,
            canResize: canResize,
            width: width,
            height: height,
            icon: icon,
            windowStartupLocation: windowStartupLocation,
            position: position,
            windowOpenedHandler: windowOpenedHandler,
            windowClosingHandler: windowClosingHandler,
            owner: owner,
            closeOnEscape: closeOnEscape);
        return task.WaitOnDispatcherFrame();
    }

    /// <summary>
    /// Показать окно
    /// </summary>
    /// <param name="userControl"> Контент окна </param>
    /// <param name="title"> Заголовок </param>
    /// <param name="canResize"> Изменение размера </param>
    /// <param name="width"> Ширина </param>
    /// <param name="height"> Высота </param>
    /// <param name="useDefaultButtons"> Использовать стандартные кнопки (Закрыть) </param>
    /// <param name="icon"> Иконка </param>
    /// <param name="windowStartupLocation"> Начально расположение окна </param>
    /// <param name="windowState"> Состояние окна </param>
    /// <param name="position"> Позиция окна, если включен ручной режим расположения </param>
    /// <param name="windowOpenedHandler"> Событие в момент открытия окна </param>
    /// <param name="windowClosingHandler"> Событие в момент начала процесса закрытия окна </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <param name="closeOnEscape"> Закрывать по нажатию на Escape </param>
    /// <returns> Окно или null </returns>
    public static Window? ShowFloatWindow(
        Control userControl,
        string title = DefaultTitle,
        bool canResize = true,
        double width = double.NaN,
        double height = double.NaN,
        bool useDefaultButtons = true,
        WindowIcon? icon = null,
        WindowStartupLocation windowStartupLocation = WindowStartupLocation.CenterScreen,
        WindowState windowState = WindowState.Normal,
        PixelPoint position = default,
        EventHandler? windowOpenedHandler = null,
        EventHandler<WindowClosingEventArgs>? windowClosingHandler = null,
        Window? ownerWindow = null,
        bool closeOnEscape = true)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return null;
        }

        var buttons = useDefaultButtons ? JupiButtonEnum.Close : (JupiButtonEnum?)null;
        var dialogWindow = useDefaultButtons || GetDialogButtons(userControl).Any()
            ? CreateWindowWithResult(userControl, buttons, title, canResize, width, height, icon, desktop.MainWindow.Icon, windowStartupLocation, windowState, position, windowOpenedHandler, windowClosingHandler)
            : CreateWindow(userControl, title, canResize, width, height, icon, desktop.MainWindow.Icon, windowStartupLocation, windowState, position, windowOpenedHandler, windowClosingHandler);
        dialogWindow.Opened += WindowOnOpened;
        if (closeOnEscape)
        {
            dialogWindow.KeyUp += DialogWindowOnKeyUp;
        }

        dialogWindow.Show(ownerWindow ?? desktop.MainWindow);
        return dialogWindow;
    }

    /// <summary>
    /// Показать окно
    /// </summary>
    /// <param name="dialogWindow"> Собственное окно </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    public static void ShowFloatWindow(Window dialogWindow, Window? ownerWindow = null)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return;
        }

        dialogWindow.Icon ??= desktop.MainWindow.Icon;
        dialogWindow.Show(ownerWindow ?? desktop.MainWindow);
    }

    /// <summary>
    /// Показать диалоговое окно с вопросом и выбором ответа (ОК, Отмена) (Асинхронно)
    /// </summary>
    /// <param name="textContent"> Текст вопроса </param>
    /// <param name="title"> Заголовок окна </param>
    /// <returns> Задача с результатом </returns>
    public static async Task<JupiDialogResult> ShowMainQuestionDialogAsync(string textContent, string title = "")
    {
        var content = new QuestionMessageControl
        {
            Title =
            {
                Text = title
            },
            ContentTextBlock =
            {
                Text = textContent
            }
        };

        var dialogSession = DialogHost.GetDialogSession(MainDialogHostString);
        if (dialogSession is not null)
        {
            return JupiDialogResult.None;
        }

        content.Focus(NavigationMethod.Tab);
        var result = await DialogHost.Show(content, MainDialogHostString);
        if (result is not JupiDialogResult dialogResult)
        {
            throw new Exception($"Не удалось привести результат диалогового окна к {nameof(JupiDialogResult)}");
        }

        return dialogResult;
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
        Window? mainWindow = null;
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindow = desktop.MainWindow;
        }

        var storage = ownerWindow is null ? mainWindow?.StorageProvider : ownerWindow.StorageProvider;
        if (storage is null || !storage.CanOpen)
        {
            // TODO:
            // Log.Warning($"Невозможно открыть проводник на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        var options = new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
        };

        if (patterns is not null)
        {
            var fileTypes = patterns.Select(pattern => new FilePickerFileType(pattern.Key) { Patterns = pattern.Value }).ToList();
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
            options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(startLocationPath));
        }

        var result = await storage.OpenFilePickerAsync(options);
        return result;
    }

    /// <summary>
    /// Открыть диалоговое окно выбора файла
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="allowMultiple"> Разрешить выбор нескольких файлов </param>
    /// <param name="patterns"> Тип выбираемых файлов - словарь: ключ - название типа файла, значение - список форматов </param>
    /// <param name="addPatternAllFiles"> Добавить паттерн для выбора всех файлов </param>
    /// <param name="startLocationPath"> Стартовое расположение проводника </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Коллекция выбранных файлов </returns>
    public static IReadOnlyList<IStorageFile>? ShowOpenFileDialog(
        string title = "Открытие",
        bool allowMultiple = false,
        IReadOnlyDictionary<string, List<string>>? patterns = null,
        bool addPatternAllFiles = false,
        string? startLocationPath = null,
        Window? ownerWindow = null)
    {
        var task = ShowOpenFileDialogAsync(
            title: title,
            allowMultiple: allowMultiple,
            patterns: patterns,
            addPatternAllFiles: addPatternAllFiles,
            startLocationPath: startLocationPath,
            ownerWindow: ownerWindow);
        return task.WaitOnDispatcherFrame();
    }

    /// <summary>
    /// Открыть диалоговое окна сохранения файла (Асинхронно)
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="suggestedFileName"> Предложенное имя файла </param>
    /// <param name="defaultExtension"> Расширение файла </param>
    /// <param name="showOverwritePrompt"> Отображает ли средство выбора открытия файла предупреждение, если пользователь указывает имя уже существующего файла </param>
    /// <param name="patterns"> Тип сохраняемых файлов - словарь: ключ - название типа файла, значение - список форматов </param>
    /// <param name="addPatternAllFiles"> Добавить паттерн для выбора всех файлов </param>
    /// <param name="startLocationPath"> Стартовое расположение проводника </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Сохраненный файл или null в случае отмены </returns>
    public static async Task<IStorageFile?> ShowSaveFileDialogAsync(
        string title = "Сохранение",
        string suggestedFileName = "",
        string defaultExtension = "",
        bool showOverwritePrompt = true,
        IReadOnlyDictionary<string, List<string>>? patterns = null,
        bool addPatternAllFiles = false,
        string? startLocationPath = null,
        Window? ownerWindow = null)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return null;
        }

        var storage = ownerWindow is null ? desktop.MainWindow.StorageProvider : ownerWindow.StorageProvider;
        if (!storage.CanOpen)
        {
            // TODO:
            //Log.Warning($"Не удалось открыть проводник на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        if (!storage.CanSave)
        {
            // TODO:
            //Log.Warning($"Невозможно произвести операцию сохранения на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        var options = new FilePickerSaveOptions
        {
            Title = title,
            ShowOverwritePrompt = showOverwritePrompt,
        };

        if (!string.IsNullOrEmpty(suggestedFileName))
        {
            options.SuggestedFileName = suggestedFileName;
        }

        if (!string.IsNullOrEmpty(defaultExtension))
        {
            options.DefaultExtension = defaultExtension;
        }

        if (patterns is not null)
        {
            var fileTypes = patterns.Select(pattern => new FilePickerFileType(pattern.Key) { Patterns = pattern.Value }).ToList();
            if (addPatternAllFiles)
            {
                var fileType = new FilePickerFileType("Все файлы")
                {
                    Patterns = new List<string> { "*.*" }
                };
                fileTypes.Add(fileType);
            }

            options.FileTypeChoices = fileTypes;
        }

        if (startLocationPath is not null)
        {
            options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(startLocationPath));
        }

        var result = await storage.SaveFilePickerAsync(options);
        return result;
    }

    /// <summary>
    /// Открыть диалоговое окна сохранения файла
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="suggestedFileName"> Предложенное имя файла </param>
    /// <param name="defaultExtension"> Расширение файла </param>
    /// <param name="showOverwritePrompt"> Отображает ли средство выбора открытия файла предупреждение, если пользователь указывает имя уже существующего файла </param>
    /// <param name="patterns"> Тип сохраняемых файлов - словарь: ключ - название типа файла, значение - список форматов </param>
    /// <param name="addPatternAllFiles"> Добавить паттерн для выбора всех файлов </param>
    /// <param name="startLocationPath"> Стартовое расположение проводника </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Сохраненный файл или null в случае отмены </returns>
    public static IStorageFile? ShowSaveFileDialog(
        string title = "Сохранение",
        string suggestedFileName = "",
        string defaultExtension = "",
        bool showOverwritePrompt = true,
        IReadOnlyDictionary<string, List<string>>? patterns = null,
        bool addPatternAllFiles = false,
        string? startLocationPath = null,
        Window? ownerWindow = null)
    {
        var task = ShowSaveFileDialogAsync(
            title: title,
            suggestedFileName: suggestedFileName,
            defaultExtension: defaultExtension,
            showOverwritePrompt: showOverwritePrompt,
            patterns: patterns,
            addPatternAllFiles: addPatternAllFiles,
            startLocationPath: startLocationPath,
            ownerWindow: ownerWindow);
        return task.WaitOnDispatcherFrame();
    }

    /// <summary>
    /// Открыть диалоговое окна выбора папки (Асинхронно)
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="allowMultiple"> Выбор нескольких папок </param>
    /// <param name="suggestedStartLocation"> Стартовое расположение </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Полный путь к выбранным папкам или null, если отмена </returns>
    public static async Task<IReadOnlyList<IStorageFolder>?> ShowOpenFolderDialogAsync(
        string title = "Выбор папки",
        bool allowMultiple = false,
        string suggestedStartLocation = "",
        Window? ownerWindow = null)
    {
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop || desktop.MainWindow is null)
        {
            return null;
        }

        var storage = ownerWindow is null ? desktop.MainWindow.StorageProvider : ownerWindow.StorageProvider;
        if (!storage.CanOpen)
        {
            // TODO:
            //Log.Warning($"Не удалось открыть проводник на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        if (!storage.CanPickFolder)
        {
            // TODO:
            //Log.Warning($"Невозможно произвести операцию выбора папки на данной операционной системе. {CommonStrings.ToDevelopers}");
            return null;
        }

        var options = new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = allowMultiple,
        };

        if (!string.IsNullOrEmpty(suggestedStartLocation))
        {
            options.SuggestedStartLocation = await storage.TryGetFolderFromPathAsync(new Uri(suggestedStartLocation));
        }

        var result = await storage.OpenFolderPickerAsync(options);
        return result;
    }

    /// <summary>
    /// Открыть диалоговое окна выбора папки
    /// </summary>
    /// <param name="title"> Заголовок </param>
    /// <param name="allowMultiple"> Выбор нескольких папок </param>
    /// <param name="suggestedStartLocation"> Стартовое расположение </param>
    /// <param name="ownerWindow"> Окно-владелец </param>
    /// <returns> Полный путь к выбранным папкам или null, если отмена </returns>
    public static IReadOnlyList<IStorageFolder>? ShowOpenFolderDialog(
        string title = "Выбор папки",
        bool allowMultiple = false,
        string suggestedStartLocation = "",
        Window? ownerWindow = null)
    {
        var task = ShowOpenFolderDialogAsync(
            title: title,
            allowMultiple: allowMultiple,
            suggestedStartLocation: suggestedStartLocation,
            ownerWindow: ownerWindow);
        return task.WaitOnDispatcherFrame();
    }

    /// <summary>
    /// Создание окна по параметрам
    /// </summary>
    /// <param name="userControl"> Контент окна </param>
    /// <param name="title"> Заголовок </param>
    /// <param name="canResize"> Изменение размера </param>
    /// <param name="width"> Начальная ширина окна </param>
    /// <param name="height"> Начальная высота окна </param>
    /// <param name="icon"> Иконка </param>
    /// <param name="defaultIcon"> Иконка по умолчанию </param>
    /// <param name="windowStartupLocation"> Начально расположение окна </param>
    /// <param name="windowState"> Состояние окна </param>
    /// <param name="position"> Позиция окна, если включен ручной режим расположения </param>
    /// <param name="windowOpenedHandler"> Событие в момент открытия окна </param>
    /// <param name="windowClosingHandler"> Событие в момент начала процесса закрытия окна </param>
    /// <returns> Окно </returns>
    private static Window CreateWindow(
        Control userControl,
        string title,
        bool canResize,
        double width,
        double height,
        WindowIcon? icon,
        WindowIcon? defaultIcon,
        WindowStartupLocation windowStartupLocation,
        WindowState windowState,
        PixelPoint position,
        EventHandler? windowOpenedHandler,
        EventHandler<WindowClosingEventArgs>? windowClosingHandler)
    {
        Application.Current.TryGetResource("AppBackColorBrush", Application.Current.ActualThemeVariant, out var resultColor);
        var window = new Window
        {
            FontSize = DefaultFontSize,
            Content = userControl,
            Title = title,
            CanResize = canResize,
            WindowStartupLocation = windowStartupLocation,
            WindowState = windowState,
            Position = position,
            Background = (SolidColorBrush)resultColor
        };

        AttachDevTools(window);
        SynchronizeWindowSize(window, userControl, width, height);

        window.Icon = icon ?? defaultIcon;
        if (windowOpenedHandler != null)
        {
            window.Opened += windowOpenedHandler;
        }

        if (windowClosingHandler != null)
        {
            window.Closing += windowClosingHandler;
        }

        return window;
    }

    /// <summary>
    /// Создание окна c результатом по параметрам
    /// </summary>
    /// <param name="userControl"> Контент окна </param>
    /// <param name="buttons"> Отображаемые кнопки </param>
    /// <param name="title"> Заголовок </param>
    /// <param name="canResize"> Изменение размера </param>
    /// <param name="width"> Начальная ширина окна </param>
    /// <param name="height"> Начальная высота окна </param>
    /// <param name="icon"> Иконка </param>
    /// <param name="defaultIcon"> Иконка по умолчанию </param>
    /// <param name="windowStartupLocation"> Начально расположение окна </param>
    /// <param name="windowState"> Состояние окна </param>
    /// <param name="position"> Позиция окна, если включен ручной режим расположения </param>
    /// <param name="windowOpenedHandler"> Событие в момент открытия окна </param>
    /// <param name="windowClosingHandler"> Событие в момент начала процесса закрытия окна </param>
    /// <returns> Окно </returns>
    private static WindowWithResult CreateWindowWithResult(
        Control userControl,
        JupiButtonEnum? buttons,
        string title,
        bool canResize,
        double width,
        double height,
        WindowIcon? icon,
        WindowIcon? defaultIcon,
        WindowStartupLocation windowStartupLocation,
        WindowState windowState,
        PixelPoint position,
        EventHandler? windowOpenedHandler,
        EventHandler<WindowClosingEventArgs>? windowClosingHandler)
    {
        Application.Current.TryGetResource("AppBackColorBrush", Application.Current.ActualThemeVariant, out var resultColor);
        var customWindow = new WindowWithResult
        {
            FontSize = DefaultFontSize,
            Title = title,
            CanResize = canResize,
            WindowStartupLocation = windowStartupLocation,
            WindowState = windowState,
            Position = position,
            Background = (SolidColorBrush)resultColor,
            contentPresenter =
            {
                Content = userControl
            }
        };

        AttachDevTools(customWindow);
        SynchronizeWindowSize(customWindow, userControl, width, height, extraHeight: customWindow.ButtonsPanelHeight);
        SynchronizeResultButtons(customWindow, userControl, buttons);
        PlaceButtons(customWindow, userControl);

        switch (buttons)
        {
            case JupiButtonEnum.Ok:
                customWindow.buttonYes.IsVisible = false;
                customWindow.buttonNo.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.YesNo:
                customWindow.buttonOk.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.OkCancel:
                customWindow.buttonYes.IsVisible = false;
                customWindow.buttonNo.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.OkAbort:
                customWindow.buttonYes.IsVisible = false;
                customWindow.buttonNo.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.YesNoCancel:
                customWindow.buttonOk.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.YesNoAbort:
                customWindow.buttonOk.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
            case JupiButtonEnum.Close:
                customWindow.buttonOk.IsVisible = false;
                customWindow.buttonYes.IsVisible = false;
                customWindow.buttonNo.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                break;
            case null:
                customWindow.buttonOk.IsVisible = false;
                customWindow.buttonYes.IsVisible = false;
                customWindow.buttonNo.IsVisible = false;
                customWindow.buttonCancel.IsVisible = false;
                customWindow.buttonClose.IsVisible = false;
                break;
        }

        customWindow.Icon = icon ?? defaultIcon;
        if (windowOpenedHandler != null)
        {
            customWindow.Opened += windowOpenedHandler;
        }

        if (windowClosingHandler != null)
        {
            customWindow.Closing += windowClosingHandler;
        }

        return customWindow;
    }

    /// <summary>
    /// Синхронизировать размеры окна с пользовательским контролом
    /// </summary>
    /// <param name="newWindow"> Окно </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    /// <param name="width"> Ширина </param>
    /// <param name="height"> Высота </param>
    /// <param name="extraWidth"> Дополнительная ширина (Например для контрола с кнопками) </param>
    /// <param name="extraHeight"> Дополнительная высота </param>
    private static void SynchronizeWindowSize(Window newWindow, Control userControl, double width, double height, double extraWidth = 0d, double extraHeight = 0d)
    {
        width = !double.IsNaN(width) ? width + extraWidth :
            !double.IsNaN(userControl.Width) ? userControl.Width + extraWidth :
            !double.IsNaN(userControl.MinWidth) ? userControl.MinWidth + extraWidth :
            !double.IsNaN(userControl.MaxWidth) ? userControl.MaxWidth + extraWidth :
            DefaultSize;

        height = !double.IsNaN(height) ? height + extraHeight :
            !double.IsNaN(userControl.Height) ? userControl.Height + extraHeight :
            !double.IsNaN(userControl.MinHeight) ? userControl.MinHeight + extraHeight :
            !double.IsNaN(userControl.MaxHeight) ? userControl.MaxHeight + extraHeight :
            DefaultSize;

        newWindow.Width = width;
        newWindow.Height = height;
        if (!double.IsNaN(userControl.MinWidth))
        {
            newWindow.MinWidth = userControl.MinWidth + extraWidth;
        }

        if (!double.IsNaN(userControl.MinHeight))
        {
            newWindow.MinHeight = userControl.MinHeight + extraHeight;
        }

        if (!double.IsNaN(userControl.MaxWidth))
        {
            newWindow.MaxWidth = userControl.MaxWidth + extraWidth;
        }

        if (!double.IsNaN(userControl.MaxHeight))
        {
            newWindow.MaxHeight = userControl.MaxHeight + extraHeight;
        }
    }

    /// <summary>
    /// Синхронизировать кнопки с результатом (Подмена кнопок окна на кнопки из контрола)
    /// </summary>
    /// <param name="customWindow"> Окно с подготовленными кнопками </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    /// <param name="buttons"> Кнопки, которые будем искать и подменять </param>
    private static void SynchronizeResultButtons(WindowWithResult customWindow, Control userControl, JupiButtonEnum? buttons)
    {
        if (buttons is null)
        {
            return;
        }

        switch (buttons)
        {
            case JupiButtonEnum.Ok:
                UseCustomOkButton(customWindow, userControl);
                break;
            case JupiButtonEnum.YesNo:
                UseCustomYesButton(customWindow, userControl);
                UseCustomNoButton(customWindow, userControl);
                break;
            case JupiButtonEnum.OkCancel:
                UseCustomOkButton(customWindow, userControl);
                UseCustomCancelButton(customWindow, userControl);
                break;
            case JupiButtonEnum.YesNoCancel:
                UseCustomCancelButton(customWindow, userControl);
                break;
        }
    }

    /// <summary>
    /// Расположить все кнопки в окне
    /// </summary>
    /// <param name="customWindow"> Окно с подготовленными кнопками </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    private static void PlaceButtons(WindowWithResult customWindow, Control userControl)
    {
        var buttons = GetDialogButtons(userControl);
        foreach (var userButton in buttons.ToArray())
        {
            var canInsert = RemoveParent(userButton);
            if (!canInsert)
            {
                buttons.Remove(userButton);
                continue;
            }

            UseCustomDialogButton(customWindow, userButton);
        }

        foreach (var child in customWindow.stackPanelRight.Children)
        {
            if (child is DialogButton dialogButton)
            {
                buttons.Add(dialogButton);
            }
        }

        var orderButtons = buttons.OrderBy(button => button.Order).ToList();
        customWindow.stackPanelRight.Children.Clear();
        foreach (var button in orderButtons)
        {
            if (button.HorizontalAlignment == HorizontalAlignment.Left)
            {
                customWindow.stackPanelLeft.Children.Add(button);
                continue;
            }

            customWindow.stackPanelRight.Children.Add(button);
        }
    }

    /// <summary>
    /// Найти кнопку типа <see cref="DialogButton"/>
    /// </summary>
    /// <param name="userControl"> Контрол в котором ищем </param>
    /// <param name="enumValue"> Параметр для поиска </param>
    /// <returns> Кнопка или null </returns>
    private static DialogButton? FindDialogButton(Control userControl, JupiDialogResult enumValue)
    {
        var buttons = new List<DialogButton>();
        foreach (var control in userControl.GetLogicalDescendants())
        {
            if (control is not DialogButton dialogButton || enumValue != dialogButton.DialogResult)
            {
                continue;
            }

            buttons.Add(dialogButton);
            break;
        }

        if (!buttons.Any())
        {
            return null;
        }

        var buttonWithLowestOrder = buttons.OrderBy(button => button.Order).First();
        return buttonWithLowestOrder;
    }

    /// <summary>
    /// Получить список кнопок
    /// </summary>
    /// <param name="userControl"> Контрол в котором ищем </param>
    /// <returns> Кнопка или null </returns>
    private static List<DialogButton> GetDialogButtons(Control userControl)
    {
        var buttons = new List<DialogButton>();
        foreach (var control in userControl.GetLogicalDescendants())
        {
            if (control is not DialogButton dialogButton)
            {
                continue;
            }

            buttons.Add(dialogButton);
        }

        return buttons;
    }

    /// <summary>
    /// Подменить кнопку
    /// </summary>
    /// <param name="customWindow"> окно с кнопками </param>
    /// <param name="windowButton"> Кнопка окна </param>
    /// <param name="userButton"> Кнопка контрола </param>
    private static void ReplaceButton(WindowWithResult customWindow, Button windowButton, Button userButton)
    {
        var canInsert = RemoveParent(userButton);
        if (!canInsert)
        {
            return;
        }

        var index = customWindow.stackPanelRight.Children.IndexOf(windowButton);
        customWindow.stackPanelRight.Children.Remove(windowButton);
        customWindow.stackPanelRight.Children.Insert(index, userButton);
    }

    /// <summary>
    /// Удалить родителя у кнопки
    /// </summary>
    /// <param name="userButton"> Пользовательская кнопка </param>
    /// <returns> Был ли удален родитель </returns>
    private static bool RemoveParent(Button userButton)
    {
        var canInsert = false;
        if (userButton.Parent is Panel panel)
        {
            panel.Children.Remove(userButton);
            canInsert = true;
        }
        else if (userButton.Parent is Border border)
        {
            border.Child = null;
            canInsert = true;
        }
        else if (userButton.Parent is ContentControl contentControl)
        {
            contentControl.Content = null;
            canInsert = true;
        }
        else if (userButton.Parent is ItemsControl itemsControl)
        {
            if (itemsControl.Items.Contains(userButton))
            {
                itemsControl.Items.Remove(userButton);
                canInsert = true;
            }
        }

        return canInsert;
    }

    /// <summary>
    /// Подменить кнопку ОК
    /// </summary>
    /// <param name="customWindow"> Кастомное окно </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    private static void UseCustomOkButton(WindowWithResult customWindow, Control userControl)
    {
        var userButton = FindDialogButton(userControl, JupiDialogResult.OK);
        if (userButton is null)
        {
            return;
        }

        ReplaceButton(customWindow, customWindow.buttonOk, userButton);
        UseCustomDialogButton(customWindow, userButton);
    }

    /// <summary>
    /// Подменить кнопку Да
    /// </summary>
    /// <param name="customWindow"> Кастомное окно </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    private static void UseCustomYesButton(WindowWithResult customWindow, Control userControl)
    {
        var userButton = FindDialogButton(userControl, JupiDialogResult.Yes);
        if (userButton is null)
        {
            return;
        }

        ReplaceButton(customWindow, customWindow.buttonYes, userButton);
        UseCustomDialogButton(customWindow, userButton);
    }

    /// <summary>
    /// Подменить кнопку Нет
    /// </summary>
    /// <param name="customWindow"> Кастомное окно </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    private static void UseCustomNoButton(WindowWithResult customWindow, Control userControl)
    {
        var userButton = FindDialogButton(userControl, JupiDialogResult.No);
        if (userButton is null)
        {
            return;
        }

        ReplaceButton(customWindow, customWindow.buttonNo, userButton);
        UseCustomDialogButton(customWindow, userButton);
    }

    /// <summary>
    /// Подменить кнопку Отмена
    /// </summary>
    /// <param name="customWindow"> Кастомное окно </param>
    /// <param name="userControl"> Пользовательский контрол </param>
    private static void UseCustomCancelButton(WindowWithResult customWindow, Control userControl)
    {
        var userButton = FindDialogButton(userControl, JupiDialogResult.Cancel);
        if (userButton is null)
        {
            return;
        }

        ReplaceButton(customWindow, customWindow.buttonCancel, userButton);
        UseCustomDialogButton(customWindow, userButton);
    }

    /// <summary>
    /// Подменить кнопку Отмена
    /// </summary>
    /// <param name="customWindow"> Кастомное окно </param>
    /// <param name="dialogButton"> Кастомная кнопка </param>
    private static void UseCustomDialogButton(WindowWithResult customWindow, DialogButton dialogButton)
    {
        switch (dialogButton.DialogResult)
        {
            case JupiDialogResult.OK:
                dialogButton.Click += customWindow.ButtonOk_OnClick;
                break;
            case JupiDialogResult.Cancel:
                dialogButton.Click += customWindow.ButtonCancel_OnClick;
                break;
            case JupiDialogResult.Yes:
                dialogButton.Click += customWindow.ButtonYes_OnClick;
                break;
            case JupiDialogResult.No:
                dialogButton.Click += customWindow.ButtonNo_OnClick;
                break;
        }
    }

    /// <summary>
    /// Открытие окна
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    private static void WindowOnOpened(object? sender, EventArgs e)
    {
        if (sender is not Window dialogWindow)
        {
            return;
        }

        TryFocusOnDialogButton(dialogWindow);
    }

    /// <summary>
    /// Попытаться сфокусироваться на <see cref="DialogButton"/>
    /// </summary>
    /// <param name="window"> Окно </param>
    private static void TryFocusOnDialogButton(Window window)
    {
        foreach (var control in window.GetLogicalDescendants())
        {
            if (control is not DialogButton dialogButton)
            {
                continue;
            }

            if (dialogButton.DialogResult != JupiDialogResult.OK || !dialogButton.IsVisible)
            {
                continue;
            }

            dialogButton.Focus(NavigationMethod.Tab);
            break;
        }
    }

    /// <summary>
    /// Отпускаем клавишу
    /// </summary>
    /// <param name="sender"> Источник </param>
    /// <param name="e"> Параметры </param>
    private static void DialogWindowOnKeyUp(object? sender, KeyEventArgs e)
    {
        if (sender is not Window window || e.Key != Key.Escape)
        {
            return;
        }

        window.Close();
    }

    /// <summary>
    /// Прикрепить к окну Avalonia.DevTools
    /// </summary>
    /// <param name="window"> Окно </param>
    private static void AttachDevTools(Window window)
    {
#if DEBUG
        window.AttachDevTools(
            new DevToolsOptions
            {
                LaunchView = DevToolsViewKind.VisualTree,
            });
#endif
    }
}