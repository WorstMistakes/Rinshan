using System;
using System.Runtime.InteropServices;

namespace TenhouDesktop
{
    static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        const uint FLASHW_ALL = 3;
        const uint FLASHW_TIMERNOFG = 12;

        [DllImport("user32.dll")]
        static extern bool FlashWindowEx(ref FLASHWINFO info);

        // Flash the taskbar button until the window comes to the foreground
        public static void FlashUntilFocused(IntPtr hwnd)
        {
            var info = new FLASHWINFO();
            info.cbSize = (uint)Marshal.SizeOf(info);
            info.hwnd = hwnd;
            info.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
            FlashWindowEx(ref info);
        }

        const uint ES_CONTINUOUS = 0x80000000;
        const uint ES_SYSTEM_REQUIRED = 0x00000001;
        const uint ES_DISPLAY_REQUIRED = 0x00000002;

        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(uint flags);

        public static void KeepDisplayOn(bool on)
        {
            SetThreadExecutionState(on ? ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED : ES_CONTINUOUS);
        }
    }
}
