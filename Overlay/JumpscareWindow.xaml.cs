using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Overlay
{
    public partial class JumpscareWindow : Window
    {
        private readonly FrameCache _cache;
        private bool _isPlaying = false;
        private MediaPlayer _screamSound = new MediaPlayer();

        internal JumpscareWindow(FrameCache cache)
        {
            InitializeComponent();
            _cache = cache;
            Visibility = Visibility.Hidden;

            var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "Assets/withered_foxy/foxy_scream.wav");
            if (File.Exists(soundPath))
            {
                _screamSound.Open(new Uri(soundPath));
            }
        }

        internal async Task PlayAndHide(byte frequency)
        {
            _isPlaying = true;

            var frames = _cache.Acquire();
            JumpscareImage.Source = frames[0];
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

            Visibility = Visibility.Visible;

            _ = PlaySound();

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

        private async Task PlaySound()
        {
            _screamSound.Play();
            await Task.Delay((int)_screamSound.NaturalDuration.TimeSpan.TotalMilliseconds);
            _screamSound.Stop();
        }
    }
}
