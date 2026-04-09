using Core;
using System.Windows;


namespace Overlay
{
    public partial class App : System.Windows.Application
    {
        private Loop _loop = new Loop();
        private NotifyIcon _trayIcon;
        private readonly FrameCache _frameCache = new FrameCache();

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _frameCache.Preload(12);

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

            _loop.OnTriggered += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    var jumpscareWindow = new JumpscareWindow(_frameCache.Frames);
                    jumpscareWindow.Show();
                });
            };

            _ = _loop.StartAsync(3);
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