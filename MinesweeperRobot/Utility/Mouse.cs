using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperRobot.Utility
{
    public static class Mouse
    {
        public static Point GetMousePosition() { return Cursor.Position; }

        public static void MoveTo(Point point)
        {
            var screen = Screen.PrimaryScreen;

            int dx = (int)Math.Ceiling((double)point.X * 65536 / (screen.Bounds.Width - 1));
            int dy = (int)Math.Ceiling((double)point.Y * 65536 / (screen.Bounds.Height - 1));

            NativeMethods.MouseEvent(0x0001 | 0x8000, dx, dy, 0, UIntPtr.Zero);
        }
        public static void MoveBy(int dx, int dy)
        {
            NativeMethods.MouseEvent(0x0001, dx, dy, 0, UIntPtr.Zero);
        }

        public static void MouseDown(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    MouseEvent(0x02); break;
                case MouseButton.Middle:
                    MouseEvent(0x20); break;
                case MouseButton.Right:
                    MouseEvent(0x08); break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }
        public static void MouseUp(MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButton.Left:
                    MouseEvent(0x04); break;
                case MouseButton.Middle:
                    MouseEvent(0x40); break;
                case MouseButton.Right:
                    MouseEvent(0x10); break;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        private static void MouseEvent(uint dwFlags)
        {
            NativeMethods.MouseEvent(dwFlags, 0, 0, 0, UIntPtr.Zero);
        }
    }

    internal static partial class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        internal static extern void MouseEvent(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);
    }

    public enum MouseButton
    {
        Left, Middle, Right
    }
}
