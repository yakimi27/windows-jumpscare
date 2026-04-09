using System.Windows;
using System.Windows.Media.Imaging;

namespace Overlay
{
    public partial class JumpscareWindow : Window
    {
        private readonly IReadOnlyList<BitmapImage> _frames;
        public JumpscareWindow(IReadOnlyList<BitmapImage> frames)
        {
            InitializeComponent();
            _frames = frames;
            JumpscareImage.Source = _frames[0];
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            foreach (BitmapImage frame in _frames)
            {
                JumpscareImage.Source = frame;
                await Task.Delay(100);
            }

            Close();
        }
    }
}
