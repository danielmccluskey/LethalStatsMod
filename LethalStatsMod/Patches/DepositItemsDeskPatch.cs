using GameNetcodeStuff;
using HarmonyLib;
using LethalStats.Models;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace LethalStats.Patches
{

    //Function used for global contributions toward global stats. This is called when the player sells items to the desk

    [HarmonyPatch(typeof(DepositItemsDesk), "CheckAllPlayersSoldItemsServerRpc")]
    public class DepositItemsDeskPatch
    {
        static bool Prefix(DepositItemsDesk __instance)
        {
            try
            {
                long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                //Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Unix Time: {unix}");


                //Make sure we haven't sent anything already in the last 10 seconds
                if (DanosLastSent.unixTime > 0)
                {
                    if (DanosLastSent.unixTime + 10 > unix)
                    {
                        return true;
                    }
                }

                //Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Sending global stats to server");
                

                if (__instance == null) return true;

                if (__instance.itemsOnCounter == null) return true;

                if (__instance.itemsOnCounter.Count == 0) return true;

                if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) return true;

                if (StartOfRound.Instance == null) return true;

                StartOfRound instance = StartOfRound.Instance;

                if (instance.localPlayerController == null) return true;


                var currentPlayerClientID = instance.localPlayerController.playerClientId;
                var currentPlayerSteamID = instance.localPlayerController.playerSteamId;

                var hostId = instance.allPlayerScripts[0].playerClientId;
                var hostSteamId = instance.allPlayerScripts[hostId].playerSteamId;

                if (hostSteamId <= 0) return true;


                Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Host Steam ID: {hostSteamId}");
                List<DanosGlobalContributions> globalContributions = new List<DanosGlobalContributions>();

                foreach (var item in __instance.itemsOnCounter)
                {
                    if (item == null) continue;

                    if (item.itemProperties == null) continue;

                    if (item.itemProperties.isScrap == false) continue;

                    if (string.IsNullOrEmpty(item.itemProperties.itemName)) continue;

                    DanosGlobalContributions contribution = new DanosGlobalContributions
                    {
                        itemName = item.itemProperties.itemName,
                        scrapValueAverage = item.scrapValue
                    };

                    globalContributions.Add(contribution);
                }

                //Remove any duplicates, but add the .count property
                globalContributions = globalContributions.GroupBy(x => x.itemName)
                    .Select(x => new DanosGlobalContributions
                    {
                        itemName = x.Key,
                        count = x.Count(),
                        //Average rounded down to the nearest whole number
                        scrapValueAverage = (int)x.Average(y => y.scrapValueAverage)
                        
                    }).ToList();


//Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Global Contributions: {globalContributions.Count}");


                //Create a dynamic object to send to the server
                var globalContribution = new
                {
                    MySteamID = currentPlayerSteamID,
                    HostSteamID = hostSteamId,
                    SoldAt = unix,
                    ItemsSold = globalContributions
                };

                

                //Post it to the global stats server https://lethalstatsservertasks.azurewebsites.net
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(globalContribution);


                //Post to the API but don't wait for a response as we don't want to block the game
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://lethalstatsservertasks.azurewebsites.net");
                //client.BaseAddress = new Uri("http://localhost:7000");
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                client.PostAsync("/api/PostGlobalContribution", content);

                DanosLastSent.unixTime = unix;
                //Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Sent global stats to server");




            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LethalStats] [DepositItemsDeskPatch]: Error in Prefix: {ex.Message}");
            }

            return true;
        }
    }



    public static class DanosLastSent
    {
        public static long unixTime { get; set; } = 0;
    }
}
