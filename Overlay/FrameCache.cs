using System.Windows.Media.Imaging;

namespace Overlay
{
    internal class FrameCache
    {
        private List<BitmapImage>? _frames;
        private readonly byte _count;
        private readonly short? _decodeWidth;
        private readonly string _entityId;

        public FrameCache(byte count, string entityId, short? decodeWidth = null)
        {
            _count = count;
            _decodeWidth = decodeWidth;
            _entityId = entityId;
        }

        public IReadOnlyList<BitmapImage> Acquire()
        {
            if (_frames != null) return _frames;

            _frames = new List<BitmapImage>();
            for (byte i = 1; i <= _count; i++)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"pack://application:,,,/Assets/{_entityId}/{i}.png");
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
