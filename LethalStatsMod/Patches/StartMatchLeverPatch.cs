using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEngine;
using Coroner;
using LethalStats.Models;
using System.Collections;

namespace LethalStats.Patches
{
    [HarmonyPatch(typeof(StartMatchLever), "PullLeverAnim")]
    public static class PullLeverAnimPatch
    {
        public static string DebugPrefix = "[LethalStats] [PullLeverAnimPatch]: ";
        static void Postfix()
        {
              
            try
            {
                StartOfRound startOfRound = StartOfRound.Instance;
                if (startOfRound == null) return;

                //If inshipphase, reset the values otherwise don't do anything
                if (startOfRound.inShipPhase)
                {
                    Debug.Log(DebugPrefix + "In ship phase, reset the values");
                    DanosPlayerStats.ResetValues();
                    UpdateHostInfo();
                    UpdateLevelName();
                    UpdateRoundStartTime();
                }
                else
                {
                    Debug.Log(DebugPrefix + "Not in ship phase, do nothing");
                }

            }
            catch (Exception ex)
            {
                Debug.Log($"{DebugPrefix}Error in Postfix: {ex.Message}");
            }
        }

       




        private static void UpdateHostInfo()
        {
            try
            {


                var instance = StartOfRound.Instance;
                if (instance == null) return;

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
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateLevelName()
        {
            try
            {
                //Get start of round instance
                var instance = StartOfRound.Instance;
                if (instance == null) return;
                SelectableLevel currentLevel = instance.currentLevel;
                if (currentLevel != null)
                {
                    DanosPlayerStats.LevelName = currentLevel.sceneName;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }

        private static void UpdateRoundStartTime()
        {
            try
            {
                DanosPlayerStats.RoundStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                Debug.Log(DebugPrefix + ex.Message);
            }
        }
    }



    //[HarmonyPatch(typeof(StartOfRound), "FirePlayersAfterDeadlineClientRpc")]
    //internal class Patch_FirePlayersAfterDeadlineClientRpc
    //{
    //    static bool Prefix(int[] endGameStats)
    //    {


    //        // Return true to continue with the original method
    //        return true;
    //    }
    //}


    

    


}

