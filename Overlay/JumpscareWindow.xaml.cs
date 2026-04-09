using System.Windows;
using System.Windows.Media.Imaging;

namespace Overlay
{
    public partial class JumpscareWindow : Window
    {
        public JumpscareWindow()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                this.Loaded += async (s, e) =>
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        JumpscareImage.Source = new BitmapImage(new Uri($"pack://application:,,,/Assets/{i}.png"));
                        await Task.Delay(40);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Close();
                    });
                };
            });
        }
    }
}
