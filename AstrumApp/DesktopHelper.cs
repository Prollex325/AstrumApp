using System;
using System.Runtime.InteropServices;

public static class DesktopHelper
{
    private const int WM_SPAWN_WORKER = 0x052C;

    [DllImport("user32.dll")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam,
        int fuFlags, int uTimeout, out IntPtr lpdwResult);

    [DllImport("user32.dll")]
    static extern IntPtr FindWindowEx(IntPtr parent, IntPtr after, string className, string windowTitle);

    [DllImport("user32.dll")]
    static extern bool SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;

    public static void AttachToDesktop(IntPtr hwnd)
    {
        // 1. Получаем Progman и "поднимаем" WorkerW
        IntPtr progman = FindWindow("Progman", null);
        if (progman == IntPtr.Zero) return;

        SendMessageTimeout(progman, WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);

        // 2. Ищем WorkerW, у которого есть SHELLDLL_DefView (это нужный нам слой)
        IntPtr workerw = IntPtr.Zero;
        IntPtr defView = IntPtr.Zero;

        do
        {
            workerw = FindWindowEx(IntPtr.Zero, workerw, "WorkerW", null);

            if (workerw != IntPtr.Zero)
            {
                defView = FindWindowEx(workerw, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (defView != IntPtr.Zero)
                {
                    // Нашли нужный WorkerW — выходим
                    break;
                }
            }
        }
        while (workerw != IntPtr.Zero);

        if (workerw == IntPtr.Zero || defView == IntPtr.Zero)
        {
            // Fallback — просто к Progman (работает, но иконки будут сверху)
            SetParent(hwnd, progman);
        }
        else
        {
            // Лучший вариант — прикрепляем к WorkerW (окно будет **под** иконками)
            SetParent(hwnd, workerw);

            // Дополнительно можно явно поставить Z-order ниже иконок
            // SetWindowPos(hwnd, defView, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        // Делаем окно видимым и без рамки (важно для виджетов)
        // Лучше делать через Win32, а не только WPF-свойства
    }
}