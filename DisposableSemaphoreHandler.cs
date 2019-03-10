// Extracted from a version of ScuffedSocket at March 10th, 2019.
// Imports not included. It is left to be said that this pattern would probably make a language designer wince, and you should think twice
// before throwing it into production code. If you want to push the boundaries even further, perhaps consider a destructor to prevent
// deadlocks. That would REALLY get a language designer pissed off. Also, maybe rename the class or something.

/*
Sample usage:

private readonly DisposableSemaphoreHandler _semaphore = new DisposableSemaphoreHandler(1, 1);

using (await _semaphore.GetWaitHandleAsync())
{
    // Your highly dangerous nuclear launch sequence code goes here.
    // (Disclaimer: I am not responsible for nuclear war or other consequences resulting from this code snippet.)
}
*/


    // Semaphore with extra fluff. i wanted to make my code slightly more readable. delete if you want, a try-finally
    // works just as well.
    internal class DisposableSemaphoreHandler : SemaphoreSlim
    {
        public DisposableSemaphoreHandler(int initialCount, int maxCount = int.MaxValue) : base(initialCount, maxCount)
        {
        }

        public async Task<IDisposable> GetWaitHandleAsync()
        {
            await WaitAsync();
            return new SemaphoreHandle(this);
        }

        internal readonly struct SemaphoreHandle : IDisposable
        {
            private readonly SemaphoreSlim _master;

            public SemaphoreHandle(SemaphoreSlim master) => _master = master;

            public void Dispose() => _master.Release();
        }
    }
