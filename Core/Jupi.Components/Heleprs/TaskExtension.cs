using Avalonia.Threading;

namespace Jupi.Components.Heleprs;

/// <summary>
/// Расширение для <see cref="Task"/>
/// </summary>
public static class TaskExtension
{
    /// <summary>
    /// Ожидание задачи синхронно в UI потоке (не блокируя его)
    /// </summary>
    /// <typeparam name="T"> Тип результата задачи </typeparam>
    /// <param name="task"> Задача </param>
    /// <returns> Результат задачи </returns>
    /// <remarks> https://github.com/AvaloniaUI/Avalonia/issues/4810 </remarks>
    [Obsolete("Следует избегать данного метода и использовать async/await", error: false)]
    public static T WaitOnDispatcherFrame<T>(this Task<T> task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, item) => ((DispatcherFrame)item!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }

        return task.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Ожидание задачи синхронно в UI потоке (не блокируя его)
    /// </summary>
    /// <param name="task"> Задача </param>
    [Obsolete("Следует избегать данного метода и использовать async/await", error: false)]
    public static void WaitOnDispatcherFrame(this Task task)
    {
        if (!task.IsCompleted)
        {
            var frame = new DispatcherFrame();
            task.ContinueWith(static (_, item) => ((DispatcherFrame)item!).Continue = false, frame);
            Dispatcher.UIThread.PushFrame(frame);
        }
    }
}