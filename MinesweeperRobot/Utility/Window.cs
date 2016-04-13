using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperRobot.Utility
{
    public static class Window
    {
        public static IntPtr GetForegroundWindow()
        {
            return NativeMethods.GetForegroundWindow();
        }

        public static bool SetForegroundWindow(IntPtr windowHandle)
        {
            return NativeMethods.SetForegroundWindow(windowHandle);
        }

        public static Rectangle GetRectangle(IntPtr windowHandle)
        {
            var rect = new NativeMethods.Rect();

            var getWindowRect = NativeMethods.GetWindowRect(windowHandle, ref rect);
            if (getWindowRect == false) throw new InvalidOperationException();

            return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left + 1, rect.Bottom - rect.Top + 1);
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr windowHandle);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref Rect rect);
        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}
