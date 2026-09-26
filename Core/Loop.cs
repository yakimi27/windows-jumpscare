using System.Diagnostics;

namespace Core
{
    public class Loop
    {
        public const int DefaultPossibility = Constants.DefaultJumpscareChance;
        private readonly Random _random = new Random();
        private bool _running;
        private volatile int _possibility;

        public event Func<Task>? OnTriggered;

        public int JumpscareChance
        {
            get => _possibility;
            set => UpdatePossibility(value);
        }

        public void UpdatePossibility(int newPossibility)
        {
            if (newPossibility <= 0)
            {
                newPossibility = DefaultPossibility;
            }
            _possibility = newPossibility;
        }

        public async Task StartAsync(int possibility)
        {
            if (possibility <= 0)
            {
                possibility = DefaultPossibility;
            }
            _possibility = possibility;
            _running = true;

            while (_running)
            {
                int currentPossibility = _possibility;
                if (currentPossibility <= 0)
                {
                    currentPossibility = DefaultPossibility;
                }

                if (_random.Next(currentPossibility) == 0)
                {
                    try
                    {
                        await Trigger();
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"Error triggering jumpscare: {ex}");
                        Debug.WriteLine($"Error triggering jumpscare: {ex}");
                    }
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
