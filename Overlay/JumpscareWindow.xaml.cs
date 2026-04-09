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
                await Task.Delay(3000);

                Dispatcher.Invoke(() =>
                {
                    Close();
                });
            });
        }
    }
}
