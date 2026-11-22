using Core;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using Desktop.Views;


namespace Desktop
{
    public partial class App : System.Windows.Application
    {
        private IConfigService _configService = null!;
        private IUserManager _userManager = null!;
        private IJumpscareManager _jumpscareManager = null!;
        private Loop _loop = new Loop();
        private NotifyIcon _trayIcon = null!;
        private JumpscareWindow? _jumpscareWindow;
        private SettingsWindow? _settingsWindow;
        private ushort _frameFrequency;
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

#if DEBUG
        private HwndSource? _hotkeyHwndSource;
        private const int DebugHotkeyId = 9001;
        private const int WmHotkey = 0x0312;
        private const uint ModControl = 0x0002;
        private const uint ModShift = 0x0004;
        private const uint ModNoRepeat = 0x4000;
        private const uint VkJ = 0x4A;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
#endif

        [DllImport("kernel32.dll")]
        private static extern bool SetProcessWorkingSetSize(IntPtr handle, IntPtr minSize, IntPtr maxSize);

        private void TrimWorkingSet()
        {
            SetProcessWorkingSetSize(
                System.Diagnostics.Process.GetCurrentProcess().Handle,
                (IntPtr)(-1),
                (IntPtr)(-1));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;

            base.OnStartup(e);

            try
            {
                _configService = new ConfigService();

                System.Drawing.Icon? trayIconImage = null;
                try
                {
                    using var iconStream = GetResourceStream(
                        new Uri("pack://application:,,,/windowsJumpscare.ico")
                    )?.Stream;
                    if (iconStream != null)
                    {
                        trayIconImage = new System.Drawing.Icon(iconStream);
                    }
                }
                catch (Exception ex)
                {
                    LogException("Failed to load tray icon resource", ex);
                }

                _trayIcon = new NotifyIcon
                {
                    Icon = trayIconImage,
                    Visible = true,
                    Text = "Windows Jumpscare"
                };

                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add("Show", null, (s, args) => ShowMainWindow());
                contextMenu.Items.Add("Trigger jumpscare", null, onClick: async (s, args) =>
                {
                    try
                    {
                        await _loop.Trigger();
                    }
                    catch (Exception ex)
                    {
                        LogException("Error triggering jumpscare from tray", ex);
                    }
                });
                contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
                _trayIcon.ContextMenuStrip = contextMenu;
                _trayIcon.DoubleClick += (s, args) => ShowMainWindow();

                _userManager = new UserManager(_configService);
                _jumpscareManager = new JumpscareManager(_configService);

                var currentJumpscare = _userManager.GetSelectedJumpscare();
                if (_jumpscareManager.GetByName(currentJumpscare) == null)
                {
                    var fallback = _jumpscareManager.GetFirstValidJumpscare();
                    if (fallback != null)
                    {
                        _userManager.SetSelectedJumpscare(fallback.Name);
                    }
                }

                _userManager.JumpscareChanged += OnJumpscareChanged;
                _userManager.JumpscareChanceChanged += OnJumpscareChanceChanged;

                try
                {
                    await LoadJumpscareAsync(_userManager.GetSelectedJumpscare());
                }
                catch (Exception ex)
                {
                    LogException("Failed to load initial jumpscare during startup", ex);
                }

                _loop.OnTriggered += () =>
                {
                    return Dispatcher.InvokeAsync(async () =>
                    {
                        await _loadLock.WaitAsync();
                        try
                        {
                            var primaryWindow = Volatile.Read(ref _jumpscareWindow);
                            if (primaryWindow != null && !primaryWindow.IsPlaying && !primaryWindow.IsClosed)
                            {
                                var cursorScreen = System.Windows.Forms.Screen.FromPoint(
                                    System.Windows.Forms.Cursor.Position);
                                var allScreens = System.Windows.Forms.Screen.AllScreens;

                                primaryWindow.PositionOnScreen(cursorScreen);

                                var extraWindows = new List<JumpscareWindow>();
                                bool matchedCursorScreen = false;
                                foreach (var screen in allScreens)
                                {
                                    if (!matchedCursorScreen && string.Equals(
                                            screen.DeviceName,
                                            cursorScreen.DeviceName,
                                            StringComparison.OrdinalIgnoreCase))
                                    {
                                        matchedCursorScreen = true;
                                        continue;
                                    }

                                    var overlay = new JumpscareWindow(
                                        primaryWindow.Cache,
                                        primaryWindow.AssetsPath,
                                        playAudio: false);
                                    await overlay.PreloadAsync();
                                    overlay.PositionOnScreen(screen);
                                    extraWindows.Add(overlay);
                                }

                                var allWindows = new List<JumpscareWindow>(1 + extraWindows.Count)
                                {
                                    primaryWindow
                                };
                                allWindows.AddRange(extraWindows);

                                var activeWindows = new List<JumpscareWindow>(allWindows.Count);
                                foreach (var window in allWindows)
                                {
                                    if (window.IsClosed) continue;

                                    try
                                    {
                                        window.Show();
                                        activeWindows.Add(window);
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        // Window may have already closed
                                    }
                                }

                                try
                                {
                                    if (activeWindows.Count > 0)
                                    {
                                        await Task.WhenAll(activeWindows.Select(w => w.PlayAndHide(_frameFrequency)));
                                    }
                                }
                                finally
                                {
                                    try
                                    {
                                        if (!primaryWindow.IsClosed)
                                        {
                                            primaryWindow.Hide();
                                        }
                                    }
                                    catch (InvalidOperationException)
                                    {
                                        // Window may have already closed
                                    }

                                    foreach (var overlay in extraWindows)
                                    {
                                        try
                                        {
                                            if (!overlay.IsClosed)
                                            {
                                                overlay.Close();
                                            }
                                        }
                                        catch (InvalidOperationException)
                                        {
                                            // Window may have already closed
                                        }
                                    }
                                }

                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                TrimWorkingSet();
                            }
                        }
                        finally
                        {
                            _loadLock.Release();
                        }
                    }).Task.Unwrap();
                };

                _ = _loop.StartAsync(_userManager.GetJumpscareChance());

#if DEBUG
                RegisterDebugHotkey();
#endif
            }
            catch (Exception ex)
            {
                LogException("Fatal error during application startup", ex);
                System.Windows.MessageBox.Show(
                    $"Windows Jumpscare encountered a fatal startup error:\n\n{ex.Message}",
                    "Windows Jumpscare - Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("Unhandled dispatcher exception", e.Exception);
            e.Handled = true;
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("Unhandled AppDomain exception", ex);
            }
            else
            {
                Trace.TraceError($"Unhandled AppDomain non-Exception object: {e.ExceptionObject}");
                Debug.WriteLine($"Unhandled AppDomain non-Exception object: {e.ExceptionObject}");
            }
        }

        private static void LogException(string context, Exception ex)
        {
            Trace.TraceError($"{context}: {ex}");
            Debug.WriteLine($"{context}: {ex}");
        }

        private async void OnJumpscareChanged(string jumpscareName)
        {
            try
            {
                await LoadJumpscareAsync(jumpscareName);
            }
            catch (Exception ex)
            {
                LogException($"Failed to load jumpscare '{jumpscareName}'", ex);
            }
        }

        private void OnJumpscareChanceChanged(int chance)
        {
            _loop.UpdatePossibility(chance);
        }

        private async Task LoadJumpscareAsync(string jumpscareName)
        {
            await _loadLock.WaitAsync();
            try
            {
                var selectedJumpscare = _jumpscareManager.GetByName(jumpscareName)
                    ?? _jumpscareManager.GetFirstValidJumpscare();
                if (selectedJumpscare == null) return;

                var oldWindow = Interlocked.Exchange(ref _jumpscareWindow, null);
                if (oldWindow != null)
                {
                    if (oldWindow.IsPlaying)
                    {
                        await oldWindow.WaitForPlaybackAsync();
                    }

                    if (!oldWindow.IsClosed)
                    {
                        try
                        {
                            oldWindow.Close();
                        }
                        catch (InvalidOperationException)
                        {
                            // Window may have already closed
                        }
                    }
                }

                FrameCache frameCache = new FrameCache(
                    selectedJumpscare.FrameAmount,
                    selectedJumpscare.AssetsPath,
                    decodeWidth: 600);

                var newWindow = new JumpscareWindow(frameCache, selectedJumpscare.AssetsPath);
                _frameFrequency = selectedJumpscare.FrameFrequency;

                await newWindow.PreloadAsync();
                Interlocked.Exchange(ref _jumpscareWindow, newWindow);

                GC.Collect();
                GC.WaitForPendingFinalizers();
                TrimWorkingSet();
            }
            finally
            {
                _loadLock.Release();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;

            base.OnExit(e);
#if DEBUG
            UnregisterDebugHotkey();
#endif
            _trayIcon?.Dispose();
        }

#if DEBUG
        private void RegisterDebugHotkey()
        {
            var parameters = new HwndSourceParameters("WindowsJumpscareDebugHotkeySink")
            {
                Width = 0,
                Height = 0,
                WindowStyle = 0
            };

            _hotkeyHwndSource = new HwndSource(parameters);
            _hotkeyHwndSource.AddHook(HotkeyWndProc);

            if (!RegisterHotKey(_hotkeyHwndSource.Handle, DebugHotkeyId, ModControl | ModShift | ModNoRepeat, VkJ))
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register debug hotkey Ctrl+Shift+J (Win32 error: {Marshal.GetLastWin32Error()})");
            }
        }

        private void UnregisterDebugHotkey()
        {
            if (_hotkeyHwndSource != null)
            {
                UnregisterHotKey(_hotkeyHwndSource.Handle, DebugHotkeyId);
                _hotkeyHwndSource.RemoveHook(HotkeyWndProc);
                _hotkeyHwndSource.Dispose();
                _hotkeyHwndSource = null;
            }
        }

        private IntPtr HotkeyWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WmHotkey && wParam.ToInt32() == DebugHotkeyId)
            {
                handled = true;
                _ = TriggerFromDebugHotkeyAsync();
            }
            return IntPtr.Zero;
        }

        private async Task TriggerFromDebugHotkeyAsync()
        {
            try
            {
                await _loop.Trigger();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error triggering jumpscare from debug hotkey: {ex}");
            }
        }
#endif

        private void ShowMainWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow(_configService, _userManager, _jumpscareManager, _loop);
                _settingsWindow.Closed += (s, e) =>
                {
                    _settingsWindow = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    TrimWorkingSet();
                };
            }
            _settingsWindow.Show();
            _settingsWindow.Activate();
        }
    }
}
