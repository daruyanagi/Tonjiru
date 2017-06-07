using System;
using System.Collections.Generic;
using System.Linq;

namespace Tonjiru
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    public static class DesktopHelper
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool EnumWindowsProcDelegate(IntPtr windowHandle, IntPtr lParam);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(
            [MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowsProcDelegate enumProc,
            IntPtr lParam);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private delegate bool EnumChildWindowsProcDelegate(IntPtr windowHandle, IntPtr lParam);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        private static extern bool EnumChildWindows(
                IntPtr handle,
                [MarshalAs(UnmanagedType.FunctionPtr)] EnumWindowsProcDelegate enumProc,
                IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public static List<TopLevelWindow> GetVisibleWindows()
        {
            // 本当は EnumWindows() でフィルタリングした方が高速だと思うが、
            // 処理の簡便のため Linq で済ます

            return GetTopLevelWindows()
                // 表示されて（いて、タイトルをもって）いるウインドウのみを列挙
                .Where(_ => _.IsVisible && !string.IsNullOrEmpty(_.Title))
                // 自分は除外しておく
                .Where(_ => _.Parent?.ProcessName.ToLower() != "tonjiru")
                // Microsoft Edge の CP（コンテンツプロセス）は除外しておく
                .Where(_ => _.Parent?.ProcessName != "MicrosoftEdgeCP")
                // Program Manager は除外しておく
                .Where(_ => _.Title != "Program Manager" && _.Parent?.ProcessName.ToLower() != "explorer")
                .ToList();
        }

        public static List<TopLevelWindow> GetTopLevelWindows()
        {
            windows.Clear();

            EnumWindows(EnumWindowProc, default(IntPtr));

            return windows;
        }
        
        private static List<TopLevelWindow> windows = new List<TopLevelWindow>();

        private static bool EnumWindowProc(IntPtr handle, IntPtr lParam)
        {
            var window = new TopLevelWindow();

            window.Handle = handle;

            window.IsVisible = IsWindowVisible(handle);

            int id = default(int);
            GetWindowThreadProcessId(handle, out id);
            window.Parent = Process.GetProcessById(id);

            var length = GetWindowTextLength(handle);
            var builder = new StringBuilder(length + 1);
            GetWindowText(handle, builder, builder.Capacity);
            window.Title = builder.ToString();

            if (window.Parent.ProcessName == "ApplicationFrameHost")
            {
                process = null;
                EnumChildWindows(handle, EnumChildWindowProc, IntPtr.Zero);
                window.Parent = process;
            }

            windows.Add(window);

            return true;
        }

        private static Process process;

        private static bool EnumChildWindowProc(IntPtr windowHandle, IntPtr lParam)
        {
            var builder = new StringBuilder(256);
            GetClassName(windowHandle, builder, builder.Capacity);

            // ストアアプリのためのハック -> "Windows.UI.Core.CoreWindow" を探す
            if (builder.ToString() == "Windows.UI.Core.CoreWindow")
            {
                int processId;
                GetWindowThreadProcessId(windowHandle, out processId);

                process = Process.GetProcessById(processId);

                if (process != null) return false;

                // Microsoft Edge は null だった。とりあえず飛ばしておくことにしておこう
            }

            return true;
        }
    }

    public class TopLevelWindow
    {
        private const int WM_CLOSE = 0x10;
        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public IntPtr Handle { get; set; }
        public bool IsVisible { get; set; }
        public string Title { get; set; }
        public Process Parent { get; set; }

        public void Close()
        {
            // とりあえず WM_CLOSE と SC_CLOSE の両方送っておく
            // ToDo：オプションで選べるようにしてもいいかも
            SendMessage(Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            SendMessage(Handle, WM_SYSCOMMAND, (IntPtr)SC_CLOSE, IntPtr.Zero);
        }
    }
}
