namespace Jupi.Components.IO;

/// <summary>
/// Класс для работы с локальными настройками программы
/// </summary>
public static class UserConfig
{
    /// <summary>
    /// Имя файла конфига с последними документами
    /// </summary>
    public const string RecentFilesConfigName = "recent.log";

    /// <summary>
    /// Имя папки приложения Jupi
    /// </summary>
    private const string JupiNodeFolderName = "JUPI NODE";

    /// <summary>
    /// Относительный путь config папки
    /// </summary>
    private static string RelativeConfigPath = Path.Combine("..", "..", "..", "config");

    /// <summary>
    /// Место хранения локальных настроек
    /// </summary>
    /// <remarks> Используется с директивой </remarks>
    private static readonly string localDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), JupiNodeFolderName, Path.GetFileNameWithoutExtension(Environment.ProcessPath));

    /// <summary>
    /// Место хранения общих для всех пользователей настроек
    /// </summary>
    /// <remarks> Используется с директивой </remarks>
    private static readonly string sharedDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), JupiNodeFolderName, Path.GetFileNameWithoutExtension(Environment.ProcessPath));

    /// <summary>
    /// Место хранения локальных настроек
    /// </summary>
    public static string Dir
    {
        get
        {
#if DEBUG
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, RelativeConfigPath));
#else
            return localDir;
#endif
        }
    }

    /// <summary>
    /// Место хранения общих настроек
    /// </summary>
    public static string SharedDir
    {
        get
        {
#if DEBUG
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, RelativeConfigPath));
#else
            return sharedDir;
#endif
        }
    }
}