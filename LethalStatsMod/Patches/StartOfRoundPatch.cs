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

namespace LethalStats.Patches
{
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    public static class StartGamePatch
    {
        static void Postfix(ref bool ___inShipPhase, ref SelectableLevel ___currentLevel)
        {
            try
            {
                DanosPlayerStats.ResetValues();
                UpdateHostInfo();
                UpdateLevelName(___currentLevel);
                UpdateRoundStartTime();
            }
            catch (Exception ex)
            {
                Debug.Log($"Error in Postfix: {ex.Message}");
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
                    Debug.Log("Host changed, reset the values");
                }
                else
                {
                    Debug.Log("Host is the same, do nothing");
                }

                DanosPlayerStats.RoundID = $"{DanosPlayerStats.HostSteamID}&&{instance.randomMapSeed}";
                DanosPlayerStats.RoundInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        private static void UpdateLevelName(SelectableLevel currentLevel)
        {
            try
            {


                if (currentLevel != null)
                {
                    DanosPlayerStats.LevelName = currentLevel.sceneName;
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
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
                Debug.Log(ex.Message);
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

