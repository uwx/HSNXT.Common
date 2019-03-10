// Extracted from ScuffedSocket at March 10th, 2019.
// This is a really compact way (if you ignore the big Try methods, but those only show up once) of registering events with failure
// handlers, useful if you have lots of them, or want to hook to them when they are being executed.
// Note that, although the try methods are static here, they may be instance methods as well.

        public event Func<MyEventArgs, Task> MyEvent;
        protected Task InvokeMyEvent(MyEventArgs args) => Try(MyEvent, args);
        
        public event Action<MySyncEventArgs> MySyncEvent;
        protected void InvokeMySyncEvent(MySyncEventArgs args) => Try(MySyncEvent, args);

        private static void Try<T>(Action<T> ev, T arg)
        {
            Console.WriteLine($"Invoking sync event {ev} with argument: [{arg}]");
            try
            {
                ev?.Invoke(arg);
            }
            catch (Exception e)
            {
                // the operation failed
            }
        }

        private static async Task Try<T>(Func<T, Task> ev, T arg)
        {
            Console.WriteLine($"Invoking event {ev} with argument: [{arg}]");
            try
            {
                if (ev != null) await ev.Invoke(arg);
            }
            catch (Exception e)
            {
                // the operation failed
            }
        }
        
// This little addition correctly implements the semantics of AsyncEventHandler (critically, the Handled property of AsyncEventArgs)
// in DSharpPlus.

        private static async Task Try<T>(AsyncEventHandler<T> ev, T arg) where T : AsyncEventArgs
        {
            Console.WriteLine($"Invoking event {ev} with argument: [{arg}]");
            try
            {
                if (ev != null)
                {
                    var delegates = ev.GetInvocationList();
                    foreach (var del in delegates)
                    {
                        if (arg.Handled) return;
                        
                        await ((AsyncEventHandler<T>)del)(arg);
                    }
                }
            }
            catch (Exception e)
            {
                // the operation failed
            }
        }
