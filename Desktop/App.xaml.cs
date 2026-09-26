using Core;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using System.Runtime.InteropServices;
using System.Windows;
using Desktop.Views;


namespace Desktop
{
    public partial class App : System.Windows.Application
    {
        private readonly IConfigService _configService = new ConfigService();
        private IUserManager _userManager = null!;
        private IJumpscareManager _jumpscareManager = null!;
        private Loop _loop = new Loop();
        private NotifyIcon _trayIcon = null!;
        private JumpscareWindow? _jumpscareWindow;
        private SettingsWindow? _settingsWindow;
        private byte _frameFrequency;
        private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

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
            base.OnStartup(e);

            using var iconStream = GetResourceStream(
                new Uri("pack://application:,,,/windowsJumpscare.ico")
            )?.Stream;

            _trayIcon = new NotifyIcon
            {
                Icon = iconStream != null ? new System.Drawing.Icon(iconStream) : null,
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
                    System.Diagnostics.Debug.WriteLine($"Error triggering jumpscare from tray: {ex}");
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

            await LoadJumpscareAsync(_userManager.GetSelectedJumpscare());

            _loop.OnTriggered += () =>
            {
                return Dispatcher.InvokeAsync(async () =>
                {
                    if (_jumpscareWindow != null && !_jumpscareWindow.IsPlaying)
                    {
                        var window = _jumpscareWindow;
                        window.Show();
                        try
                        {
                            await window.PlayAndHide(_frameFrequency);
                        }
                        finally
                        {
                            try
                            {
                                window.Hide();
                            }
                            catch (InvalidOperationException)
                            {
                                // Window may have already closed
                            }
                        }

                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        TrimWorkingSet();
                    }
                }).Task.Unwrap();
            };

            _ = _loop.StartAsync(_userManager.GetJumpscareChance());
        }

        private async void OnJumpscareChanged(string jumpscareName)
        {
            try
            {
                await LoadJumpscareAsync(jumpscareName);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load jumpscare '{jumpscareName}': {ex.Message}");
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

                if (_jumpscareWindow != null)
                {
                    if (_jumpscareWindow.IsPlaying)
                    {
                        await _jumpscareWindow.WaitForPlaybackAsync();
                    }

                    _jumpscareWindow.Close();
                    _jumpscareWindow = null;
                }

                FrameCache frameCache = new FrameCache(
                    selectedJumpscare.FrameAmount,
                    selectedJumpscare.AssetsPath,
                    decodeWidth: 600);

                var newWindow = new JumpscareWindow(frameCache, selectedJumpscare.AssetsPath);
                _frameFrequency = selectedJumpscare.FrameFrequency;

                await newWindow.PreloadAsync();
                _jumpscareWindow = newWindow;

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
            base.OnExit(e);
            _trayIcon?.Dispose();
        }

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
