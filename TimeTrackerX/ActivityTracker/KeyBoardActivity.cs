using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Input;

namespace TimeTrackerX.ActivityTracker
{
    public class InterceptKeys
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_ALTDOWN = 0x0104;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Custom KeyEventArgs to avoid Windows Forms dependency
        public class KeyEventArgs : EventArgs
        {
            public Key Key { get; }
            public bool Handled { get; set; }

            public KeyEventArgs(Key key)
            {
                Key = key;
            }
        }

        public static void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _hookID = SetHook(_proc);
            }
            else
            {
                // Placeholder for macOS/Linux
                Console.WriteLine("Global keyboard hooks not implemented for this platform.");
                // Integrate local capture if needed (see LocalStart)
            }
        }

        public static void Stop()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                UnhookWindowsHookEx(_hookID);
            }
            // No-op for macOS/Linux unless implemented
        }

        public static event EventHandler<KeyEventArgs> OnKeyDown;

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    WH_KEYBOARD_LL,
                    proc,
                    GetModuleHandle(curModule.ModuleName),
                    0
                );
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_ALTDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                // Map Windows VK code to Avalonia Key (simplified)
                Key key = MapVirtualKeyToAvaloniaKey(vkCode);
                var args = new KeyEventArgs(key);
                OnKeyDown?.Invoke(null, args);
                if (args.Handled)
                    return (IntPtr)1; // Block key if handled
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // Simplified VK to Avalonia Key mapping (extend as needed)
        private static Key MapVirtualKeyToAvaloniaKey(int vkCode)
        {
            // Map common keys (incomplete; add more mappings based on usage)
            switch (vkCode)
            {
                case 0x41:
                    return Key.A; // A key
                case 0x42:
                    return Key.B; // B key
                case 0x1B:
                    return Key.Escape;
                case 0x0D:
                    return Key.Enter;
                case 0x12:
                    return Key.LeftAlt; // Alt
                case 0x20:
                    return Key.Space;
                // Add more VK codes: https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes
                default:
                    return Key.None; // Unknown key
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
