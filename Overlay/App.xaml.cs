using Core;
using System.Windows;


namespace Overlay
{
    public partial class App : System.Windows.Application
    {
        private Loop _loop = new Loop();
        private NotifyIcon _trayIcon;
        private readonly FrameCache _frameCache = new FrameCache(15, "withered_foxy", decodeWidth: 600);
        private JumpscareWindow? _jumpscareWindow;

        private const byte _frameFrequency = 60; //milliseconds
        private const ushort _jumpscareChance = 3; //max value 65535

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
            contextMenu.Items.Add("Exit", null, (s, args) => Shutdown());
            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, args) => ShowMainWindow();

            _jumpscareWindow = new JumpscareWindow(_frameCache);

            _loop.OnTriggered += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    _jumpscareWindow.PlayAndHide(_frameFrequency);
                });
            };

            _ = _loop.StartAsync(_jumpscareChance);
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