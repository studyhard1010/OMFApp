using System;
using System.Timers;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace BartApp
{
    class BartCapture
    {
        private static Timer aTimer;
        private static BartOMF bartOMF;
        private static BartScheduleGetter bartScheduleGetter;

        public void Run()
        {
            // Read in configuration.
            IConfigurationBuilder builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json");
            IConfiguration configuration = builder.Build();

            bartOMF = new BartOMF();
            bartOMF.initializeConfigure(configuration);
            // Create PI OMF type and containers.
            bartOMF.createTypesAndContainers();

            bartScheduleGetter = new BartScheduleGetter();
            bartScheduleGetter.initBartUrl(configuration);
            var schedule = bartScheduleGetter.GetBartSchedule();
            Console.WriteLine("XXXXX--------: Get Bart Schedule <{0}>", JsonConvert.SerializeObject(schedule));
            bartOMF.createDataValues(schedule);

            // Create a timer with a one minute interval.
            aTimer = new System.Timers.Timer(1 * 60 * 1000);
            //Timer aTimer = new Timer(10 * 1000);

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        public void Stop()
        {
            aTimer.Stop();
            aTimer.Dispose();
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);

            var schedule = bartScheduleGetter.GetBartSchedule();
            Console.WriteLine("XXXXX--------: Get Bart Schedule <{0}>", JsonConvert.SerializeObject(schedule));
            bartOMF.createDataValues(schedule);
        }
    }
}
