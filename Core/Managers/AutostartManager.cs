using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Core.Managers
{
    [SupportedOSPlatform("windows")]
    public static class AutostartManager
    {
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "WindowsJumpscare";

        public static void SetAutostart(bool enable)
        {
            if (!OperatingSystem.IsWindows()) return;

            using var key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
            if (key == null) return;

            if (enable)
            {
                string? exePath = Environment.ProcessPath;

                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName, throwOnMissingValue: false);
                }
            }
        }

        public static bool IsAutostartEnabled()
        {
            if (!OperatingSystem.IsWindows()) return false;

            using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            return key?.GetValue(AppName) != null;
        }
    }
}
