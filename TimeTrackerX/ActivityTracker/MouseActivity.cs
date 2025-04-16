using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;

namespace TimeTrackerX.ActivityTracker
{
    public static class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall
        )]
        public static extern int SetWindowsHookEx(
            int idHook,
            HookProc lpfn,
            IntPtr hInstance,
            int threadId
        );

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall
        )]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport(
            "user32.dll",
            CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall
        )]
        public static extern int CallNextHookEx(
            int idHook,
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );
    }

    public class MouseHook
    {
        private Point point;
        private Point Point
        {
            get => point;
            set
            {
                if (point != value)
                {
                    point = value;
                    MouseMoveEvent?.Invoke(
                        this,
                        new MouseEventArgs(MouseButton.None, 0, point.X, point.Y, 0)
                    );
                }
            }
        }

        private int hHook;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        public const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEWHEEL = 0x020A;
        private Win32Api.HookProc hProc;

        // Custom MouseEventArgs to avoid Windows Forms dependency
        public class MouseEventArgs : EventArgs
        {
            public MouseButton Button { get; }
            public int Clicks { get; }
            public double X { get; }
            public double Y { get; }
            public int Delta { get; }
            public bool Handled { get; set; }

            public MouseEventArgs(MouseButton button, int clicks, double x, double y, int delta)
            {
                Button = button;
                Clicks = clicks;
                X = x;
                Y = y;
                Delta = delta;
            }
        }

        public MouseHook()
        {
            this.point = new Point();
        }

        public void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                hHook = SetHook();
            }
            else
            {
                // Placeholder for macOS/Linux
                Console.WriteLine("Global mouse hooks not implemented for this platform.");
                // Use LocalStart for app-focused capture if needed
            }
        }

        public void Stop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UnHook();
            }
            // No-op for macOS/Linux
        }

        private int SetHook()
        {
            hProc = MouseHookProc;
            hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
            return hHook;
        }

        private void UnHook()
        {
            Win32Api.UnhookWindowsHookEx(hHook);
        }

        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var myMouseHookStruct = (Win32Api.MouseHookStruct)
                Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
            if (nCode < 0)
            {
                return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }

            MouseButton button = MouseButton.None;
            int clickCount = 0;
            int delta = 0;

            switch ((int)wParam)
            {
                case WM_LBUTTONDOWN:
                    button = MouseButton.Left;
                    clickCount = 1;
                    MouseDownEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_RBUTTONDOWN:
                    button = MouseButton.Right;
                    clickCount = 1;
                    MouseDownEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_MBUTTONDOWN:
                    button = MouseButton.Middle;
                    clickCount = 1;
                    MouseDownEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_LBUTTONUP:
                    button = MouseButton.Left;
                    clickCount = 1;
                    MouseUpEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_RBUTTONUP:
                    button = MouseButton.Right;
                    clickCount = 1;
                    MouseUpEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_MBUTTONUP:
                    button = MouseButton.Middle;
                    clickCount = 1;
                    MouseUpEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_LBUTTONDBLCLK:
                    button = MouseButton.Left;
                    clickCount = 2;
                    MouseClickEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_RBUTTONDBLCLK:
                    button = MouseButton.Right;
                    clickCount = 2;
                    MouseClickEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_MBUTTONDBLCLK:
                    button = MouseButton.Middle;
                    clickCount = 2;
                    MouseClickEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            clickCount,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
                case WM_MOUSEWHEEL:
                    button = MouseButton.Middle;
                    delta = (short)((int)lParam >> 16); // Simplified wheel delta
                    MouseWheelEvent?.Invoke(
                        this,
                        new MouseEventArgs(
                            button,
                            0,
                            myMouseHookStruct.pt.x,
                            myMouseHookStruct.pt.y,
                            delta
                        )
                    );
                    break;
            }

            this.Point = new Point(myMouseHookStruct.pt.x, myMouseHookStruct.pt.y);
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
        public event MouseMoveHandler MouseMoveEvent;

        public delegate void MouseClickHandler(object sender, MouseEventArgs e);
        public event MouseClickHandler MouseClickEvent;

        public delegate void MouseDownHandler(object sender, MouseEventArgs e);
        public event MouseDownHandler MouseDownEvent;

        public delegate void MouseUpHandler(object sender, MouseEventArgs e);
        public event MouseUpHandler MouseUpEvent;

        public delegate void MouseWheelHandler(object sender, MouseEventArgs e);
        public event MouseWheelHandler MouseWheelEvent;
    }
}
