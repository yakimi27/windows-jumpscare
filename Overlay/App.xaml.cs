using Core;
using Core.Interfaces;
using Core.Managers;
using Core.Services;
using System.Runtime.InteropServices;
using System.Windows;


namespace Overlay
{
    public partial class App : System.Windows.Application
    {
        private Loop _loop = new Loop();
        private NotifyIcon _trayIcon;
        private JumpscareWindow? _jumpscareWindow;

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

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Jumpscare App"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, args) => ShowMainWindow());
            contextMenu.Items.Add("Trigger jumpscare", null, onClick: async (s, args) => await _loop.Trigger());
            contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, args) => ShowMainWindow();

            IConfigService configService = new ConfigService();
            IUserManager userManager = new UserManager(configService);
            IJumpscareManager jumpscareManager = new JumpscareManager(configService);

            var selectedJumpscare = jumpscareManager.GetByName(userManager.GetSelectedJumpscare());

            FrameCache frameCache = new FrameCache(selectedJumpscare.FrameAmount,
                selectedJumpscare.AssetsPath, decodeWidth: 600);

            _jumpscareWindow = new JumpscareWindow(frameCache, selectedJumpscare.AssetsPath);
            var frameFrequency = selectedJumpscare.FrameFrequency;
            var selectedJumpscarePath = selectedJumpscare.AssetsPath;

            //preload
            await _jumpscareWindow.PreloadAsync();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            TrimWorkingSet();

            _loop.OnTriggered += () =>
            {
                Dispatcher.Invoke(async () =>
                {
                    _jumpscareWindow.Show();
                    await _jumpscareWindow.PlayAndHide(frameFrequency);

                    _jumpscareWindow.Hide();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    TrimWorkingSet();
                });
            };

            _ = _loop.StartAsync(userManager.GetJumpscareChance());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _trayIcon?.Dispose();
        }

        private void ShowMainWindow()
        {
            MainWindow?.Show();
            MainWindow?.Activate();
        }
    }
}