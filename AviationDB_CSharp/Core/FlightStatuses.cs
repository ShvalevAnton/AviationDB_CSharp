using System;
using System.Collections.Generic;
using System.Text;

namespace AviationDB_CSharp.Core
{
    public class FlightStatuses
    {
        public static List<string> AllStatuses => new List<string>
        {
            "Scheduled",
            "On Time",
            "Delayed",
            "Departed",
            "Arrived",
            "Cancelled"
        };
    }
}
