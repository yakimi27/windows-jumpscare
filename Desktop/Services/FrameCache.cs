using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Desktop
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

            var frames = new List<BitmapImage>();
            for (int i = 1; i <= _frameQuantity; i++)
            {
                var frame = CreateFrame(i);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            _frames = frames;
            return _frames;
        }

        public async Task PreloadAsync()
        {
            if (_frames != null) return;

            await Task.Run(() =>
            {
                var frames = new List<BitmapImage>();
                for (int i = 1; i <= _frameQuantity; i++)
                {
                    var frame = CreateFrame(i);
                    if (frame != null)
                    {
                        frames.Add(frame);
                    }
                }
                _frames = frames;
            });
        }

        private BitmapImage? CreateFrame(int index)
        {
            try
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
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to load frame {index} from '{_entityAssetsPath}': {ex}");
                Debug.WriteLine($"Failed to load frame {index} from '{_entityAssetsPath}': {ex}");
                return null;
            }
        }

        public void Release()
        {
            _frames?.Clear();
            _frames = null;
        }
    }
}
