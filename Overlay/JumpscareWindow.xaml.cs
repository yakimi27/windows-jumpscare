using System.Windows;

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

        internal async void PlayAndHide(byte frequency)
        {
            if (_isPlaying) return;
            _isPlaying = true;

            var frames = _cache.Acquire();
            Visibility = Visibility.Visible;

            foreach (var frame in frames)
            {
                JumpscareImage.Source = frame;
                await Task.Delay(frequency);
            }

            JumpscareImage.Source = null;
            Visibility = Visibility.Hidden;
            _cache.Release();
            _isPlaying = false;
        }
    }
}
