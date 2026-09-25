namespace Core
{
    public class Loop
    {
        public const int DefaultPossibility = Constants.DefaultJumpscareChance;
        private readonly Random _random = new Random();
        private bool _running;
        private volatile int _posibility;

        public event Func<Task>? OnTriggered;

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
                    await Trigger();
                }

                await Task.Delay(3000);
            }
        }

        public async Task Trigger()
        {
            if (OnTriggered != null)
            {
                var delegates = OnTriggered.GetInvocationList();
                var tasks = new Task[delegates.Length];
                for (int i = 0; i < delegates.Length; i++)
                {
                    tasks[i] = ((Func<Task>)delegates[i])() ?? Task.CompletedTask;
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}
