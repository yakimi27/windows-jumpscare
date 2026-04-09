using System.Windows.Media.Imaging;

namespace Overlay
{
    internal class FrameCache
    {
        private readonly List<BitmapImage> _frame = new List<BitmapImage>();
        public IReadOnlyList<BitmapImage> Frames => _frame;

        public void Preload(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri($"pack://application:,,,/Assets/{i}.png");
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.EndInit();
                bitmap.Freeze();
                _frame.Add(bitmap);
            }
        }
    }
}
