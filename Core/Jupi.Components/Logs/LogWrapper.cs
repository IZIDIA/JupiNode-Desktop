using Jupi.Components.IO;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using System.Text;

namespace Jupi.Components.Logs;

/// <summary>
/// Обертка дл работы с NLog.
/// </summary>
internal class LogWrapper
{
    /// <summary>
    /// Логер NLog
    /// </summary>
    private readonly Logger _logger;

    /// <summary>
    /// Конфигурирование логера
    /// </summary>
    public LogWrapper()
    {
        LogFolder = Path.Combine(UserConfig.Dir, "log");
        var config = new LoggingConfiguration();
        var csvTarget = new FileTarget
        {
            Encoding = Encoding.UTF8,
            FileName = Path.Combine(LogFolder, "csv-${shortdate}.log"),
            ArchiveFileName = Path.Combine(LogFolder, "log.{#}.txt"),
            ArchiveEvery = FileArchivePeriod.Day,
            ArchiveNumbering = ArchiveNumberingMode.Rolling,
            MaxArchiveFiles = 7,
            ConcurrentWrites = true,
        };
        var csvLayout = new CsvLayout
        {
            Delimiter = CsvColumnDelimiterMode.Tab,
            WithHeader = true
        };
        csvLayout.Columns.Add(new CsvColumn("time", "${longdate}"));
        csvLayout.Columns.Add(new CsvColumn("level", "${level:upperCase=true}"));
        csvLayout.Columns.Add(new CsvColumn("message", "${message}"));
        csvLayout.Columns.Add(new CsvColumn("callsite", "${callsite:includeSourcePath=true}"));
        csvLayout.Columns.Add(new CsvColumn("stacktrace", "${stacktrace:topFrames=10}"));
        csvLayout.Columns.Add(new CsvColumn("exception", "${exception:format=ToString}"));
        csvTarget.Layout = csvLayout;
        var rule2 = new LoggingRule("*", LogLevel.Debug, csvTarget);
        config.LoggingRules.Add(rule2);
        config.AddTarget("csv-file", csvTarget);
        LogManager.Configuration = config;
        _logger = LogManager.GetCurrentClassLogger();
    }

    /// <summary>
    /// Папка с логами
    /// </summary>
    public string LogFolder { get; }

    /// <summary>
    /// Вывод информации об ошибке
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="ex"> Ошибка </param>
    public void Error(string message, Exception? ex = null)
    {
        if (ex is null)
        {
            _logger.Error(message);
        }
        else
        {
            _logger.Error(ex, message);
        }
    }

    /// <summary>
    /// Вывод информационного сообщения
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    public void Info(string message)
    {
        _logger.Info(message);
    }

    /// <summary>
    /// Вывод сообщения предупреждения
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    public void Warn(string message)
    {
        _logger.Warn(message);
    }

    /// <summary>
    /// Сообщения для отладки
    /// </summary>
    /// <param name="message"> Текстовое сообщение </param>
    /// <param name="ex"> Ошибка </param>
    public void Debug(string message, Exception? ex = null)
    {
        if (ex == null)
        {
            _logger.Debug(message);
        }
        else
        {
            _logger.Debug(ex, message);
        }
    }
}