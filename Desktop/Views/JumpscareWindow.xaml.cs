using System.Diagnostics;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Desktop.Views
{
    public partial class JumpscareWindow : Window
    {
        private readonly FrameCache _cache;
        private readonly string _assetsPath;
        private readonly string _soundPath;
        private bool _isPlaying = false;
        private bool _isClosed = false;
        private SoundPlayer? _screamSound;
        private bool _isSoundLoaded = false;
        private System.Drawing.Rectangle? _targetScreenBounds;
        private IReadOnlyList<BitmapImage> _frames = Array.Empty<BitmapImage>();
        private TaskCompletionSource<bool>? _playbackTcs;

        internal bool IsPlaying => _isPlaying;
        internal FrameCache Cache => _cache;
        internal string AssetsPath => _assetsPath;

        internal JumpscareWindow(FrameCache cache, string assetsPath, bool playAudio = true)
        {
            InitializeComponent();
            _cache = cache;
            _assetsPath = assetsPath;
            Visibility = Visibility.Hidden;

            _soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                $"{assetsPath}/scream.wav");

            if (playAudio && File.Exists(_soundPath))
            {
                _screamSound = new SoundPlayer(_soundPath);
            }
        }

        /// <summary>
        /// Positions and sizes the window to cover the specified monitor,
        /// translating screen bounds to WPF DIPs using the window's DPI scale.
        /// </summary>
        internal void PositionOnScreen(System.Windows.Forms.Screen screen)
        {
            _targetScreenBounds = screen.Bounds;
            ApplyScreenBounds(screen.Bounds, VisualTreeHelper.GetDpi(this));
        }

        private void ApplyScreenBounds(System.Drawing.Rectangle bounds, DpiScale dpi)
        {
            Left = bounds.Left / dpi.DpiScaleX;
            Top = bounds.Top / dpi.DpiScaleY;
            Width = bounds.Width / dpi.DpiScaleX;
            Height = bounds.Height / dpi.DpiScaleY;
        }

        protected override void OnDpiChanged(DpiScale oldDpi, DpiScale newDpi)
        {
            base.OnDpiChanged(oldDpi, newDpi);

            if (_targetScreenBounds is { } bounds)
            {
                ApplyScreenBounds(bounds, newDpi);
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

            await PreloadSoundAsync();
        }

        private async Task PreloadSoundAsync()
        {
            if (_screamSound == null || _isClosed) return;

            try
            {
                await Task.Run(() => _screamSound.Load());
                _isSoundLoaded = true;
            }
            catch (Exception ex)
            {
                _isSoundLoaded = false;
                Debug.WriteLine($"Failed to preload scream sound '{_soundPath}': {ex}");
            }
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

        internal async Task PlayAndHide(ushort frequency)
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

                PlaySound();

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

        private void PlaySound()
        {
            if (_screamSound == null || _isClosed) return;

            try
            {
                if (!_isSoundLoaded)
                {
                    _screamSound.Load();
                    _isSoundLoaded = true;
                }

                _screamSound.Play();
            }
            catch (Exception ex)
            {
                _isSoundLoaded = false;
                Debug.WriteLine($"Failed to play scream sound '{_soundPath}': {ex}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _isClosed = true;
            _isPlaying = false;
            _playbackTcs?.TrySetResult(true);

            if (_screamSound != null)
            {
                try
                {
                    _screamSound.Stop();
                    _screamSound.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to dispose scream sound '{_soundPath}': {ex}");
                }
                finally
                {
                    _screamSound = null;
                    _isSoundLoaded = false;
                }
            }
        }
    }
}
