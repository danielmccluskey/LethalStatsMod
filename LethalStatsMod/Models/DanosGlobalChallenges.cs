using System;
using System.Collections.Generic;
using System.Text;

namespace LethalStats.Models
{
    public static class DanosGlobalChallenges
    {
        public static bool ShownMessageThisSession { get; set; } = false;
        public static DanosGlobalChallenge GlobalChallenge { get; set; }
    }


    public class GlobalChallengeList
    {
        public DanosGlobalChallenge[] Property1 { get; set; }
    }

    public class DanosGlobalChallenge
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string TerminalText { get; set; }
        public string ChallengeType { get; set; }
        public int Target { get; set; }
        public int CurrentProgress { get; set; }
        public int StartDateUTC { get; set; }
        public int EndDateUTC { get; set; }
        public string Criteria { get; set; }
    }



}
