using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Desktop
{
    public partial class JumpscareWindow : Window
    {
        private readonly FrameCache _cache;
        private bool _isPlaying = false;
        private MediaPlayer _screamSound = new MediaPlayer();
        private IReadOnlyList<BitmapImage> _frames;

        internal JumpscareWindow(FrameCache cache, string assetsPath)
        {
            InitializeComponent();
            _cache = cache;
            Visibility = Visibility.Hidden;

            var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{assetsPath}/scream.wav");

            if (File.Exists(soundPath))
            {
                _screamSound.Open(new Uri(soundPath));
                _screamSound.Volume = 0; //muted for priming
            }
        }

        internal async Task PreloadAsync()
        {
            _frames = await _cache.PreloadAsync().ContinueWith(_ => _cache.Acquire());

            // prime
            if (_frames.Count > 0)
            {
                JumpscareImage.Source = _frames[0];
            }

            _screamSound.Play();
            _screamSound.Stop();
            _screamSound.Volume = 1.0;
        }

        internal async Task PlayAndHide(byte frequency)
        {
            _isPlaying = true;

            JumpscareImage.Source = _frames[0];
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

            Visibility = Visibility.Visible;

            _ = PlaySound();

            foreach (var frame in _frames)
            {
                JumpscareImage.Source = frame;
                await Task.Delay(frequency);
            }

            JumpscareImage.Source = null;
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

            Visibility = Visibility.Hidden;
            _isPlaying = false;
        }

        private async Task PlaySound()
        {
            _screamSound.Position = TimeSpan.FromMilliseconds(1);
            _screamSound.Position = TimeSpan.Zero;
            _screamSound.Play();
        }
    }
}
