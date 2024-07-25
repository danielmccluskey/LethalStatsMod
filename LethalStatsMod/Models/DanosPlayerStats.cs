using Coroner;
using LethalStats.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace LethalStats.Models
{
    //Messy class to store all the stats, will hopefully clean this up later
    public class DanosPlayerStats
    {
        public static string RoundID = "";//HostSteamID + StartOfRound.Instance.roundStartTime.ToString();

        public static DateTime LastSentResults = DateTime.MinValue;

        public static string LevelName { get; set; } = "Unknown";

        public static List<DanosPlayerEvent> PlayerEvents = new List<DanosPlayerEvent>();
        public static List<DanosPlayerItem> PlayerItems = new List<DanosPlayerItem>();
        public static bool HasSentResults = false;

        public static bool RoundInitialized = false;

        public static long RoundStart = 0;
        public static long RoundEnd = 0;
        public static double RoundDuration
        {
            get
            {
                //Get total minutes from the start and end time
                return (RoundEnd - RoundStart) / 60.0;

            }
        }



        public static long QuotaStringA = 0;//Quota fulfilled
        public static long QuotaStringB = 0;//Target quota
        public static long QuotaStringC = 0;//Times fulfilled quota

        public static int TotalScrapCollectedThisRound { get; set; }
        public static float TotalScrapOnMap { get; set; }

        public static int DaysOnTheJob { get; set; }
        public static float ScrapOnShip { get; set; }
        public static int ScrapValueCollected { get; set; }
        public static int TeamDeaths { get; set; }
        public static int TeamStepsTaken { get; set; }

        public static int CreditsAtEnd { get; set; }
        public static int PlayersInLobby { get; set; }
        public static int PlayersDead { get; set; }

        public static int StepsTaken { get; set; }

        public static ulong MySteamID { get; set; }
        public static string MyUsername { get; set; }

        public static ulong HostSteamID { get; set; }

        public static bool Fired { get; set; } = false;


        public static int Deaths { get; set; }

        public static Dictionary<AdvancedCauseOfDeath?, int> deathCounts = new Dictionary<AdvancedCauseOfDeath?, int>();
        public static void IncrementDeathCount(AdvancedCauseOfDeath? cause)
        {
            if (!deathCounts.ContainsKey(cause))
            {
                deathCounts[cause] = 0;
            }
            deathCounts[cause]++;

            Deaths++;
        }

        public static int GetDeathCount(AdvancedCauseOfDeath? cause)
        {
            return deathCounts.ContainsKey(cause) ? deathCounts[cause] : 0;
        }

        public static void ResetDeathCounts()
        {
            deathCounts.Clear();
        }


        public static bool PostResults()
        {
            try
            {


                //Check if results were sent in the last 30 seconds to avoid duplicates and wasteful calls
                if ((DateTime.Now - LastSentResults).TotalSeconds < 30)
                {
                    Debug.Log("Results were sent in the last 30 seconds, skipping");
                    return true;
                }

                //if deaths is greater than 0, get the first cause of death
                string causeOfDeath = "";
                if (Deaths > 0)
                {
                    causeOfDeath = deathCounts.FirstOrDefault().Key.ToString();
                }

                var gamenumber = 49;
                try
                {


                    GameNetworkManager gameNetworkManager = GameNetworkManager.Instance;
                    if (gameNetworkManager != null)
                    {
                        gamenumber = gameNetworkManager.gameVersionNum;
                    }
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }

                //Collect users mods to send to the API for challenges.
                List<DanosPlayerMods> mods = new List<DanosPlayerMods>();
                try
                {
                    mods = DanosPlayerMods.CollectMods();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                    mods = new();
                }







                var staticData = new
                {
                    RoundID = DanosPlayerStats.RoundID,
                    HostSteamID = DanosPlayerStats.HostSteamID,
                    DaysOnTheJob = DanosPlayerStats.DaysOnTheJob,
                    ScrapValueCollected = DanosPlayerStats.ScrapValueCollected,
                    TeamDeaths = DanosPlayerStats.TeamDeaths,
                    TeamStepsTaken = DanosPlayerStats.TeamStepsTaken,
                    Deaths = DanosPlayerStats.Deaths,
                    StepsTaken = DanosPlayerStats.StepsTaken,
                    MySteamID = DanosPlayerStats.MySteamID,
                    MyUsername = DanosPlayerStats.MyUsername,
                    QuotaStringA = DanosPlayerStats.QuotaStringA,
                    QuotaStringB = DanosPlayerStats.QuotaStringB,
                    QuotaStringC = DanosPlayerStats.QuotaStringC,
                    ScrapOnShip = DanosPlayerStats.ScrapOnShip,
                    LevelName = DanosPlayerStats.LevelName,
                    PlayerDied = DanosPlayerStats.Deaths > 0,
                    CauseOfDeath = causeOfDeath,
                    RoundStart = DanosPlayerStats.RoundStart,
                    RoundEnd = DanosPlayerStats.RoundEnd,
                    RoundDuration = DanosPlayerStats.RoundDuration,
                    CreditsAtEnd = DanosPlayerStats.CreditsAtEnd,
                    PlayersInLobby = DanosPlayerStats.PlayersInLobby,
                    PlayersDead = DanosPlayerStats.PlayersDead,
                    TotalScrapCollectedThisRound = DanosPlayerStats.TotalScrapCollectedThisRound,
                    TotalScrapOnMap = DanosPlayerStats.TotalScrapOnMap,
                    Fired = DanosPlayerStats.Fired,
                    GameVersion = gamenumber,
                    Events = new List<DanosPlayerEvent>(),
                    Mods = mods,
                    Items = DanosPlayerStats.PlayerItems




                };
                //Convert all the static variables to a JSON string
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(staticData);


                //Post to the API but don't wait for a response as we don't want to block the game
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://lethalstatsmiddleman.azurewebsites.net");
                //client.BaseAddress = new Uri("http://localhost:7023");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.PostAsync("/api/PostResults?", content);


                //Set the last sent results to now
                LastSentResults = DateTime.Now;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return false;
            }
            return true;
        }



        public static void ResetValues()
        {
            DanosPlayerStats.RoundID = "";
            DanosPlayerStats.PlayerEvents = new List<DanosPlayerEvent>();
            DanosPlayerStats.PlayerItems = new List<DanosPlayerItem>();
            DanosPlayerStats.HasSentResults = false;
            DanosPlayerStats.DaysOnTheJob = 0;
            DanosPlayerStats.ScrapValueCollected = 0;
            DanosPlayerStats.TeamDeaths = 0;
            DanosPlayerStats.TeamStepsTaken = 0;
            DanosPlayerStats.Deaths = 0;
            DanosPlayerStats.StepsTaken = 0;
            DanosPlayerStats.MySteamID = 0;
            DanosPlayerStats.MyUsername = "";
            DanosPlayerStats.QuotaStringA = 0;
            DanosPlayerStats.QuotaStringB = 0;
            DanosPlayerStats.QuotaStringC = 0;
            DanosPlayerStats.ScrapOnShip = 0;
            DanosPlayerStats.LevelName = "Unknown";
            DanosPlayerStats.RoundStart = 0;
            DanosPlayerStats.RoundEnd = 0;
            DanosPlayerStats.CreditsAtEnd = 0;
            DanosPlayerStats.PlayersInLobby = 0;
            DanosPlayerStats.PlayersDead = 0;
            DanosPlayerStats.TotalScrapCollectedThisRound = 0;
            DanosPlayerStats.TotalScrapOnMap = 0;
            DanosPlayerStats.Fired = false;
            ResetDeathCounts();


        }





    }
}
