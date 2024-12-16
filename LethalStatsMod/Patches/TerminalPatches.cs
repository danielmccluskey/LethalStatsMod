using HarmonyLib;
using LethalStats.Models;
using System;
using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

namespace LethalStats.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    public static partial class Events
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void Awake(ref Terminal __instance)
        {
            TerminalPatches.Terminal = __instance;
        }
    }


    [HarmonyPatch(typeof(Terminal), "Start")]
    public static class TerminalPatches
    {
        public static Terminal Terminal { get; internal set; }

        // Postfix method to start the coroutine
        static void Postfix(Terminal __instance)
        {
            __instance.StartCoroutine(GatherGlobalStatsCoroutine(__instance));
        }

        // Coroutine wrapper for the async task
        public static IEnumerator GatherGlobalStatsCoroutine(Terminal a_term)
        {
            var task = GatherGlobalStats(a_term);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogError($"[LethalStats] [TerminalPatches]: Error in GatherGlobalStats: {task.Exception.Message}");
            }
        }

        private static IEnumerator ShowChallengeAvailableCoroutine()
        {
            yield return new WaitForSeconds(5);

            try
            {
                if(DanosGlobalChallenges.ShownMessageThisSession)
                {
                    //End coroutine if message has already been shown
                    yield break;
                }

                HUDManager hUDManager = HUDManager.Instance;
                if (hUDManager != null)
                {
                    hUDManager.DisplayTip("LethalStats", "New Orders available, use command \"global\" in the terminal.");
                    DanosGlobalChallenges.ShownMessageThisSession = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LethalStats] [TerminalPatches]: Error in ShowChallengeAvailableCoroutine: {ex.Message}");
            }
        }

        // Asynchronous method to gather global stats
        public static async Task GatherGlobalStats(Terminal a_term)
        {
            string DisplayText = "We are gathering the latest global challenge information. Please wait...";
            string terminalWord = "global";

            TerminalNode terminalNode = ScriptableObject.CreateInstance<TerminalNode>();
            terminalNode.displayText = DisplayText;
            terminalNode.clearPreviousText = true;
            terminalNode.terminalEvent = "";


            //Get the terminal object
            TerminalKeyword terminalKeyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            terminalKeyword.word = terminalWord;
            terminalKeyword.isVerb = false;
            terminalKeyword.specialKeywordResult = terminalNode;



            //Try and find the terminal script
            var terminal = Terminal;
            if (terminal != null)
            {

                //Check if the terminal object has the terminal keyword
                if (terminal.terminalNodes != null)
                {
                    //Check if the terminal keyword is already in the list
                    if (terminal.terminalNodes.allKeywords.FirstOrDefault(x => x.word == terminalWord) == null)
                    {
                        //Add the terminal keyword to the list
                        terminal.terminalNodes.allKeywords = terminal.terminalNodes.allKeywords.AddItem(terminalKeyword).ToArray();
                        Console.WriteLine($"[LethalStats] [TerminalPatches]: Terminal object has terminalKeyword");
                    }
                    else
                    {
                        Console.WriteLine($"[LethalStats] [TerminalPatches]: Terminal object already has terminalKeyword");
                    }
                }
                else
                {
                    Console.WriteLine($"[LethalStats] [TerminalPatches]: Terminal object does not have terminalNodes");
                }


            }
            else
            {
                Console.WriteLine($"[LethalStats] [TerminalPatches]: Terminal object is null");
            }

            bool giveNotification = false;

            try
            {




                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://lethalstatsservertasks.azurewebsites.net/api/GetCurrentGlobalChallenges");
                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {

                        var content = await response.Content.ReadAsStringAsync();
                        // Process the content, deserialize JSON, etc.
                        Debug.Log(content);

                        var globalChallenges = Newtonsoft.Json.JsonConvert.DeserializeObject<DanosGlobalChallenge[]>(content);
                        if (globalChallenges == null)
                        {
                            Console.WriteLine($"[LethalStats] [TerminalPatches]: GlobalChallenges is null");
                            return;
                        }

                        else
                        {
                            //Get the first result from the List
                            var first = globalChallenges.FirstOrDefault();
                            if (first == null)
                            {
                                return;
                            }
                            else
                            {
                                DanosGlobalChallenges.GlobalChallenge = first;
                                Console.WriteLine($"[LethalStats] [TerminalPatches]: GlobalChallenge: {first.Title}");
                                giveNotification = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LethalStats] [TerminalPatches]: Error in GatherGlobalStats: {ex.Message}");
            }

            DisplayText = "No global challenge available at this time. Please check back later.";


            if (DanosGlobalChallenges.GlobalChallenge != null)
            {
                terminalWord = "global";
                DisplayText = $"{DanosGlobalChallenges.GlobalChallenge.TerminalText}";

            }



            try
            {


                //Edit the terminal node
                terminalNode.displayText = DisplayText;
                terminalNode.clearPreviousText = true;

                //Edit the terminal keyword
                terminalKeyword.word = terminalWord;

                //update it in the terminal object
                for (int i = 0; i < terminal.terminalNodes.allKeywords.Length; i++)
                {
                    if (terminal.terminalNodes.allKeywords[i].word == terminalWord)
                    {
                        terminal.terminalNodes.allKeywords[i] = terminalKeyword;
                    }
                }
                if (giveNotification)
                {


                    //Show the challenge available message
                    a_term.StartCoroutine(ShowChallengeAvailableCoroutine());
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LethalStats] [TerminalPatches]: Error in GatherGlobalStats: {ex.Message}");
            }







        }

    }
}
