namespace Core
{
    public class Loop
    {
        private readonly Random _random = new Random();
        private bool _running;

        public event Action? OnTriggered;

        public async Task StartAsync(int posibility)
        {
            var _running = true;

            while (_running)
            {
                if (_random.Next(posibility) == 0)
                {
                    OnTriggered?.Invoke();
                }

                await Task.Delay(3000);
            }
        }
    }
}
