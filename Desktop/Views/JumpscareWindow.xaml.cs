using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Desktop.Views
{
    public partial class JumpscareWindow : Window
    {
        private readonly FrameCache _cache;
        private bool _isPlaying = false;
        private bool _isClosed = false;
        private MediaPlayer _screamSound = new MediaPlayer();
        private IReadOnlyList<BitmapImage> _frames = Array.Empty<BitmapImage>();
        private TaskCompletionSource<bool>? _playbackTcs;

        internal bool IsPlaying => _isPlaying;

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
            await _cache.PreloadAsync();
            _frames = _cache.Acquire();

            // prime
            if (_frames.Count > 0)
            {
                JumpscareImage.Source = _frames[0];
            }

            _screamSound.Play();
            _screamSound.Stop();
            _screamSound.Volume = 1.0;
        }

        internal async Task WaitForPlaybackAsync()
        {
            while (_isPlaying)
            {
                var tcs = _playbackTcs;
                if (tcs != null)
                {
                    await Task.WhenAny(tcs.Task, Task.Delay(5000));
                }
                else
                {
                    await Task.Delay(50);
                }
            }
        }

        internal async Task PlayAndHide(byte frequency)
        {
            if (_isPlaying || _isClosed) return;
            _isPlaying = true;
            _playbackTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            try
            {
                if (_isClosed) return;
                JumpscareImage.Source = _frames.Count > 0 ? _frames[0] : null;
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

                if (_isClosed) return;
                Visibility = Visibility.Visible;

                _ = PlaySound();

                foreach (var frame in _frames)
                {
                    if (_isClosed) return;
                    JumpscareImage.Source = frame;
                    await Task.Delay(frequency);
                }

                if (_isClosed) return;
                JumpscareImage.Source = null;
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);

                if (!_isClosed)
                {
                    Visibility = Visibility.Hidden;
                }
            }
            finally
            {
                _isPlaying = false;
                _playbackTcs?.TrySetResult(true);
            }
        }

        private async Task PlaySound()
        {
            _screamSound.Position = TimeSpan.FromMilliseconds(1);
            _screamSound.Position = TimeSpan.Zero;
            _screamSound.Play();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _isClosed = true;
            _isPlaying = false;
            _playbackTcs?.TrySetResult(true);
            _screamSound.Stop();
            _screamSound.Close();
        }
    }
}
