using GameNetcodeStuff;
using HarmonyLib;
using LethalStats.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalStats.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB), "SetItemInElevator")]
    public class SetItemInElevator_Prefix
    {
        static bool Prefix(PlayerControllerB __instance, bool droppedInShipRoom, bool droppedInElevator, GrabbableObject gObject)
        {
            try
            {
                //Quick check to make sure we have a valid object
                if(gObject == null) return true;
                if (gObject.itemProperties == null) return true;
                if(gObject.itemProperties.isScrap == false) return true;
                if (string.IsNullOrEmpty(gObject.itemProperties.itemName)) return true;
                if (gObject.isInShipRoom == droppedInShipRoom)
                    return true;
                if (gObject.scrapPersistedThroughRounds)
                    return true;
                if (!droppedInShipRoom)
                {
                    //check if this item is already in the list
                    if (DanosPlayerStats.PlayerItems.Exists(x => x.Id == gObject.itemProperties.itemId && x.ItemName == gObject.itemProperties.itemName))
                    {
                        //Remove the item from the list
                        DanosPlayerStats.PlayerItems.Remove(DanosPlayerStats.PlayerItems.Find(x => x.Id == gObject.itemProperties.itemId && x.ItemName == gObject.itemProperties.itemName));
                    }
                    return true;
                }

                DanosPlayerItem item = new DanosPlayerItem
                {
                    Id = gObject.itemProperties.itemId,
                    ItemName = gObject.itemProperties.itemName,
                    ItemValue = gObject.scrapValue,
                    CreditsWorth = gObject.itemProperties.creditsWorth,
                    CollectedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                DanosPlayerStats.PlayerItems.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LethalStats] [SetItemInElevator_Prefix]: Error in Prefix: {ex.Message}");
            }
            

            return true;
        }
    }
}
