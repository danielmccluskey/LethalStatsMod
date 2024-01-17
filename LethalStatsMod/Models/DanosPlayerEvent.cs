using System;
using System.Collections.Generic;
using System.Text;

namespace LethalStats.Models
{
    //Unused class for player events, will hopefully use this later
    public class DanosPlayerEvent
    {
        public string EventName { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> AdditionalDetails { get; set; }

        public DanosPlayerEvent(string eventName, Dictionary<string, string> additionalDetails = null)
        {
            EventName = eventName;
            Timestamp = DateTime.UtcNow;
            AdditionalDetails = additionalDetails ?? new Dictionary<string, string>();
        }
    }
}
