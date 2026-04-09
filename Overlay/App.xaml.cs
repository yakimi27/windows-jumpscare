using Core;
using System.Windows;

namespace Overlay
{
    public partial class App : Application
    {
        private Loop _loop = new Loop();
        private NotifyIcon _notifyIcon;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _loop.OnTriggered += () =>
            {
                Dispatcher.Invoke(() =>
                {
                    var jumpscareWindow = new JumpscareWindow();
                    jumpscareWindow.Show();
                });

            };

            _ = _loop.StartAsync(1);
        }
    }

}
