namespace Core
{
    public class Loop
    {
        private const int DefaultPossibility = 749635;
        private readonly Random _random = new Random();
        private bool _running;
        private volatile int _posibility;

        public event Action? OnTriggered;

        public int JumpscareChance
        {
            get => _posibility;
            set => UpdatePossibility(value);
        }

        public void UpdatePossibility(int newPossibility)
        {
            if (newPossibility <= 0)
            {
                newPossibility = DefaultPossibility;
            }
            _posibility = newPossibility;
        }

        public async Task StartAsync(int posibility)
        {
            if (posibility <= 0)
            {
                posibility = DefaultPossibility;
            }
            _posibility = posibility;
            _running = true;

            while (_running)
            {
                int currentPossibility = _posibility;
                if (currentPossibility <= 0)
                {
                    currentPossibility = DefaultPossibility;
                }

                if (_random.Next(currentPossibility) == 0)
                {
                    OnTriggered?.Invoke();
                }

                await Task.Delay(3000);
            }
        }
        public Task Trigger()
        {
            OnTriggered?.Invoke();
            return Task.CompletedTask;
        }
    }
}
