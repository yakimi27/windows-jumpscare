using System.Windows.Media.Imaging;

namespace Overlay
{
    internal class FrameCache
    {
        private List<BitmapImage>? _frames;
        private readonly byte _frameQuantity;
        private readonly short? _decodeWidth;
        private readonly string _entityAssetsPath;

        public FrameCache(byte frameQuantity, string entityAssetsPath, short? decodeWidth = null)
        {
            _frameQuantity = frameQuantity;
            _decodeWidth = decodeWidth;
            _entityAssetsPath = entityAssetsPath;
        }

        public IReadOnlyList<BitmapImage> Acquire()
        {
            if (_frames != null) return _frames;

            _frames = new List<BitmapImage>();
            for (byte i = 1; i <= _frameQuantity; i++)
            {
                _frames.Add(CreateFrame(i));
            }
            return _frames;
        }

        public async Task PreloadAsync()
        {
            if (_frames != null) return;

            await Task.Run(() =>
            {
                var frames = new List<BitmapImage>();
                for (byte i = 1; i <= _frameQuantity; i++)
                {
                    frames.Add(CreateFrame(i));
                }
                _frames = frames;
            });
        }

        private BitmapImage CreateFrame(byte index)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri($"pack://application:,,,/{_entityAssetsPath}/{index}.png");
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.None;
            if (_decodeWidth.HasValue)
                bitmap.DecodePixelWidth = _decodeWidth.Value;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public void Release()
        {
            _frames = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
