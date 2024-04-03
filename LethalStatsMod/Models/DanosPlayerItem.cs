using System;
using System.Collections.Generic;
using System.Text;

namespace LethalStats.Models
{
    public class DanosPlayerItem
    {
        public int Id { get; set; } = 0;
        public string ItemName { get; set; } = "";
        public int ItemValue { get; set; } = 0;
        public int CreditsWorth { get; set; } = 0;
        public long CollectedAt { get; set; } = 0;
    }
}
