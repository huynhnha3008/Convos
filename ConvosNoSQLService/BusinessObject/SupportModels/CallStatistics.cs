using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessObject.SupportModels
{
    public class CallStatistics
{
    public int TotalCalls { get; set; }
    public int MissedCalls { get; set; }
    public double AverageCallDuration { get; set; }
    public Dictionary<string, int> CallsByType { get; set; }
}
}