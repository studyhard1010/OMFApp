using System;
using System.Timers;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace BartApp
{
    class Program
    {
        static void Main()
        {
            bool keepRunning = true;
            BartCapture capture = new BartCapture();
            capture.Run();

            Console.WriteLine("\nPress the Ctrl-c to exit the application...\n");
            Console.WriteLine("The application started at {0:HH:mm:ss.fff}", DateTime.Now);

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) {
                e.Cancel = true;
                keepRunning = false;
            };

            while (keepRunning)
            {
                Thread.Sleep(500);
            }

            capture.Stop();
            Console.WriteLine("Terminating the application...");
        }
    }
}
