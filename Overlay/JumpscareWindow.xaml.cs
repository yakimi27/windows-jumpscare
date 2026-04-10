using System.Windows;
using System.Windows.Threading;

namespace Overlay
{
    public partial class JumpscareWindow : Window
    {
        private readonly FrameCache _cache;
        private bool _isPlaying = false;

        internal JumpscareWindow(FrameCache cache)
        {
            InitializeComponent();
            _cache = cache;
            Visibility = Visibility.Hidden;
        }

        internal async Task PlayAndHide(byte frequency)
        {
            _isPlaying = true;

            var frames = _cache.Acquire();
            JumpscareImage.Source = frames[0];
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

            Visibility = Visibility.Visible;

            foreach (var frame in frames)
            {
                JumpscareImage.Source = frame;
                await Task.Delay(frequency);
            }

            JumpscareImage.Source = null;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            _cache.Release();
            _isPlaying = false;
            // caller does  close + null after this returns
        }
    }
}
