using Coroner;
using LethalStats.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LethalStats.Patches
{
    [HarmonyLib.HarmonyPatch(typeof(HUDManager))]
    [HarmonyLib.HarmonyPatch("FillEndGameStats")]
    class HUDManagerFillEndGameStatsPatch
    {
        public static string DebugPrefix = "[LethalStatsMod] [HUDManagerFillEndGameStatsPatch]: ";
        public static void Prefix()
        {
            try
            {
                bool foundRoundStart = true;
                // Initialize StartOfRound instance
                var instance = StartOfRound.Instance;
                if (instance == null) return;


                //Check that hostid and roundid are set for failsafe
                if (DanosPlayerStats.HostSteamID <= 0 || DanosPlayerStats.RoundID == null)
                {
                                        var hostId = instance.allPlayerScripts[0].playerClientId;
                    var hostSteamId = instance.allPlayerScripts[hostId].playerSteamId;

                    if (hostSteamId <= 0) return;

                    var hostChanged = hostSteamId != DanosPlayerStats.HostSteamID;
                    DanosPlayerStats.HostSteamID = hostSteamId;

                    if (hostChanged)
                    {
                        Debug.Log(DebugPrefix+"Host changed, reset the values");
                    }
                    else
                    {
                        Debug.Log(DebugPrefix + "Host is the same, do nothing");
                    }

                    DanosPlayerStats.RoundID = $"{DanosPlayerStats.HostSteamID}&&{instance.randomMapSeed}";
                    DanosPlayerStats.RoundInitialized = true;


                    if (DanosPlayerStats.RoundStart == 0)
                    {
                        foundRoundStart = false;
                    }


                }


                UpdatePlayerStats(instance);
                UpdateTimeOfDayStats();
                UpdateScrapOnShip();
                UpdateRoundEnd();
                UpdateCreditsAtEnd();
                UpdateTotalScrap(instance);
                UpdateNetworkPlayerStats(instance);
                UpdateTeamStats(instance);
                UpdateFired();


                if(foundRoundStart == false)
                {
                    //Set start time to end time
                    DanosPlayerStats.RoundStart = DanosPlayerStats.RoundEnd;
                }

                // Post results and reset values
                DanosPlayerStats.PostResults();
                DanosPlayerStats.ResetValues();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        //Get DaysOnJob and TeamDeathCount and TeamStepsTaken
        private static void UpdateTeamStats(StartOfRound instance)
        {
            try
            {
                var teamStats = instance.gameStats;
                if (teamStats != null)
                {

                    DanosPlayerStats.DaysOnTheJob = teamStats.daysSpent;

                    DanosPlayerStats.TeamDeaths = teamStats.deaths;

                    var allplayerstats = teamStats.allPlayerStats;
                    if (allplayerstats != null)
                    {
                        foreach (var player in allplayerstats)
                        {
                            if(player.isActivePlayer)
                            {
                                DanosPlayerStats.TeamStepsTaken += player.stepsTaken;
                            }
                        }
                    }




                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }



        //Estimate if they are fired or not
        private static void UpdateFired()
        {
            try
            {
                //Get the TimeOfDay instance
                TimeOfDay timeOfDay = TimeOfDay.Instance;
                if (timeOfDay == null) return;

                //Is it the last day?
                bool flag = timeOfDay.daysUntilDeadline <= 0f;

                Debug.Log(DebugPrefix + "TimeUntilDeadline: " + timeOfDay.daysUntilDeadline);

                //If it is not the last day, then they are not fired
                if (flag == false)
                {
                    DanosPlayerStats.Fired = false;
                    return;
                }

                //If it is the last day, then check if they are fired
                //If QuotaStringA is greater than B, then they are not fired
                if (DanosPlayerStats.QuotaStringA >= DanosPlayerStats.QuotaStringB)
                {
                    DanosPlayerStats.Fired = false;
                }
                else
                {
                    DanosPlayerStats.Fired = true;
                }





                
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }






        //Taken from the ShipLoot mod by tinyhoot
        private static float CalculateLootValue()
        {
            try
            {


                GameObject ship = GameObject.Find("/Environment/HangarShip");

                var loot = ship.GetComponentsInChildren<GrabbableObject>()
                    .Where(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem").ToList();
                return loot.Sum(scrap => scrap.scrapValue);
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
                return 0;
            }
        }
        private static void UpdatePlayerStats(StartOfRound instance)
        {
            try
            {
                var currentPlayerClientID = instance.localPlayerController.playerClientId;
                var currentPlayerStats = instance.gameStats.allPlayerStats[currentPlayerClientID];
                DanosPlayerStats.StepsTaken = currentPlayerStats.stepsTaken;
                UpdatePlayerDeathStats(instance);
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdatePlayerDeathStats(StartOfRound instance)
        {
            try
            {
                if (instance.localPlayerController.isPlayerDead)
                {

                    var causeOfDeath = AdvancedDeathTracker.GetCauseOfDeath(instance.localPlayerController);
                    if (causeOfDeath != null)
                    {
                        DanosPlayerStats.IncrementDeathCount(causeOfDeath);
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateTimeOfDayStats()
        {
            try
            {
                TimeOfDay timeOfDay = TimeOfDay.Instance;
                if (timeOfDay != null)
                {
                    DanosPlayerStats.QuotaStringA = timeOfDay.quotaFulfilled;
                    DanosPlayerStats.QuotaStringB = timeOfDay.profitQuota;
                    DanosPlayerStats.QuotaStringC = timeOfDay.timesFulfilledQuota;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateScrapOnShip()
        {
            try
            {
                DanosPlayerStats.ScrapOnShip = CalculateLootValue();
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateRoundEnd()
        {
            try
            {
                DanosPlayerStats.RoundEnd = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateCreditsAtEnd()
        {
            try
            {
                var terminal = GameObject.FindObjectOfType<Terminal>();
                if (terminal != null)
                {
                    DanosPlayerStats.CreditsAtEnd = terminal.groupCredits;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateTotalScrap(StartOfRound instance)
        {
            try
            {
                var roundInstance = RoundManager.Instance;
                if (roundInstance != null)
                {
                    DanosPlayerStats.TotalScrapOnMap = roundInstance.totalScrapValueInLevel;
                    DanosPlayerStats.TotalScrapCollectedThisRound = roundInstance.scrapCollectedInLevel;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateNetworkPlayerStats(StartOfRound instance)
        {
            try
            {
                if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) return;

                var currentPlayerClientID = instance.localPlayerController.playerClientId;
                var currentPlayerSteamID = instance.localPlayerController.playerSteamId;
                var currentPlayerUsername = instance.localPlayerController.playerUsername;


                DanosPlayerStats.MySteamID = currentPlayerSteamID;
                DanosPlayerStats.MyUsername = currentPlayerUsername;

                // Check if instance.gameStats.allPlayerStats[] exists for the current client id index
                if (instance.gameStats.allPlayerStats.Count() > (int)currentPlayerClientID)
                {
                    var currentPlayerStats = instance.gameStats.allPlayerStats[currentPlayerClientID];
                    DanosPlayerStats.StepsTaken = currentPlayerStats.stepsTaken;
                }

                try
                {
                    var activePlayers = instance.allPlayerScripts.Where(x => x.playerSteamId > 0).ToList();
                    DanosPlayerStats.PlayersInLobby = activePlayers.Count;
                    DanosPlayerStats.PlayersDead = activePlayers.Count(x => x.isPlayerDead);
                }
                catch (Exception ex)
                {
                    Debug.Log(DebugPrefix + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }

        }
    }

}
