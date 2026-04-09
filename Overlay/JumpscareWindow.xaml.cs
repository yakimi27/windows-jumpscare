using System.Windows;

namespace Overlay
{
    public partial class JumpscareWindow : Window
    {
        public JumpscareWindow()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                await Task.Delay(1000);

                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            });
        }
    }
}
