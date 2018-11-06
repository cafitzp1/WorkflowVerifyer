using System;
using System.Threading;

namespace WorkflowVerifyer.App.Helpers
{
    public class ConsoleSpinner : IDisposable
    {
        private const string Sequence = @"/-\|";
        private int counter = 0;
        private readonly int left;
        private readonly int top;
        private readonly int delay;
        private bool active;
        private readonly Thread thread;

        public ConsoleSpinner(int left, int top, int delay = 100)
        {
            this.left = left;
            this.top = top;
            this.delay = delay;
            thread = new Thread(Spin);
        }
        public void Start(string msg = "")
        {
            if (msg.Length > 0) Console.Write(msg);

            active = true;
            if (!thread.IsAlive)
                thread.Start();
        }
        public void Stop(string msg = " ")
        {
            active = false;
            Draw(msg);
        }
        private void Spin()
        {
            Console.Write("-\n");
            //Console.CursorVisible = false;
            while (active)
            {
                Turn();
                Thread.Sleep(delay);
            }
            //Console.CursorVisible = true;
        }
        private void Draw(String str)
        {
            Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            Console.Write($"{str}\n");
        }
        private void Turn()
        {
            Draw(Sequence[++counter % Sequence.Length].ToString());
        }
        public void Dispose()
        {
            Stop();
        }
    }
}