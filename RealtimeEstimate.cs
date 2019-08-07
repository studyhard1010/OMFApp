using System.Collections.Generic;

namespace BartApp
{
    public class BartSchedule
    {
        public string minutes { get; set; }
        public string platform { get; set; }
        public string direction { get; set; }
        public string length { get; set; }
        public string color { get; set; }
        public string hexcolor { get; set; }
        public string bikeflag { get; set; }
        public string delay { get; set; }
    }

    public class Etd
    {
        public string destination { get; set; }
        public string abbreviation { get; set; }
        public string limited { get; set; }
        public List<BartSchedule> estimate { get; set; }
    }

    public class Station
    {
        public string name { get; set; }
        public string abbr { get; set; }
        public List<Etd> etd { get; set; }
    }

    public class RealtimeEstimate
    {
        public string date { get; set; }
        public string time { get; set; }
        public List<Station> station { get; set; }
    }

    public class RealtimeEstimateObject
    {
        public RealtimeEstimate root { get; set; }
    }

    ////////////////////////////////////////////////////////////
    // Bart etc schedule OMF define 
    public class OMFBartETDSchedule
    {
        public string datetime { get; set; }
        public string origName { get; set; }
        public string origAbbr { get; set; }
        public string destName { get; set; } = "";
        public string destAbbr { get; set; } = "";

        public float minutes { get; set; }
        public int platform { get; set; }
        public string direction { get; set; }
        public float length { get; set; }
        public float delay { get; set; }

        //public BartSchedule estimate { get; set; }
    }

    public class OMFBartDataValues
    {
        public string containerid { get; set; }
        public List<OMFBartETDSchedule> values { get; set; }
    }

}