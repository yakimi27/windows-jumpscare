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
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"pack://application:,,,/{_entityAssetsPath}/{i}.png");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                if (_decodeWidth.HasValue)
                    bitmap.DecodePixelWidth = _decodeWidth.Value;
                bitmap.EndInit();
                bitmap.Freeze();
                _frames.Add(bitmap);
            }
            return _frames;
        }

        public void Release()
        {
            _frames = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
