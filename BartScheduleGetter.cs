using System;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BartApp
{
    class BartScheduleGetter
    {
        private static string bartURL;

        public bool initBartUrl(IConfiguration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            var url = configuration["BartURL"];
            var orig = configuration["BartOrigStation"];
            var key = configuration["BartURLKey"];

            bartURL = String.Format(@"{0}/api/etd.aspx?cmd=etd&orig={1}&key={2}&json=y", url, orig, key);
            Console.WriteLine("Bart URL: {0}", bartURL);

            return true;
        }

        private static async Task<string> SendBartRequest()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                using (HttpResponseMessage response = await client.GetAsync(bartURL))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error sending OMF response code:{response.StatusCode}.  Response {responseBody}");
                    }
                    return responseBody;
                }
            }
        }

        private string MakeGoodDate(string bartDate, string bartTime)
        {
            var dateVal = DateTime.Parse(bartDate);
            var timeSplit = bartTime.Split(' ');
            var timeVal = TimeSpan.Parse(timeSplit[0]);
            dateVal = dateVal.Add(timeVal);
            if (timeSplit[1].StartsWith("PM"))
                dateVal = dateVal.AddHours(12);
            return dateVal.ToString("s");
        }

        public List<OMFBartETDSchedule> GetBartSchedule()
        {
            try
            {
                List<OMFBartETDSchedule> etdScheduleArray = new List<OMFBartETDSchedule>();

                string response = SendBartRequest().Result;
                RealtimeEstimateObject rb = JsonConvert.DeserializeObject<RealtimeEstimateObject>(response);
                Console.WriteLine("Realtime estimates time: {0} {1}.", rb.root.date, rb.root.time);

                // Get first station estimate info.
                Station station = rb.root.station[0];

                Console.Write("Station name: {0} abbr: {1}.\n", station.name, station.abbr);
                Console.Write("****************************************************************************************************************\n");

                foreach (Etd etd in station.etd)
                {
                    if (etd.estimate.Count == 0)
                    {
                        Console.WriteLine("Destination station name {0} abbr {1} etc empty...",
                            etd.destination, etd.abbreviation);
                        continue;
                    }

                    OMFBartETDSchedule etc = new OMFBartETDSchedule();
                    etc.datetime = MakeGoodDate(rb.root.date, rb.root.time);
                    etc.origName = station.name;
                    etc.origAbbr = station.abbr;
                    etc.minutes = Int32.Parse(etd.estimate[0].minutes);
                    etc.platform = Int32.Parse(etd.estimate[0].platform);
                    etc.direction = etd.estimate[0].direction;
                    etc.length = Int32.Parse(etd.estimate[0].length);
                    etc.delay = Int32.Parse(etd.estimate[0].delay);
                    //etc.estimate = etd.estimate[0];
                    etdScheduleArray.Add(etc);

                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //// for test print
                    Console.Write("ETD destination: {0} abbreviation: {1} limited: {2}.\n",
                       etd.destination, etd.abbreviation, etd.limited);

                    foreach (BartSchedule estimate in etd.estimate)
                    {
                        Console.WriteLine("Please note if the train is leaving, then the estimated minutes is 0.");
                        Console.Write("Estimate minutes: {0} platform: {1} direction: {2} length: {3} color: {4} hexcolor: {5} bikeflag: {6} delay: {7}.\n",
                            estimate.minutes, estimate.platform, estimate.direction, estimate.length,
                            estimate.color, estimate.hexcolor, estimate.bikeflag, estimate.delay);
                    }

                    Console.Write("****************************************************************************************************************\n");
                    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }

                return etdScheduleArray;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
