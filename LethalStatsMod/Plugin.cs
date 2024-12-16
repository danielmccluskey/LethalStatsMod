using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using LethalStats.Patches;

namespace LethalStats
{
    public class PluginInfo
    {
        public const string PLUGIN_GUID = "com.danos.lethalstats";
        public const string PLUGIN_NAME = "LethalStats";
        public const string PLUGIN_VERSION = "1.0.1";
    }



    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.elitemastereric.coroner", BepInDependency.DependencyFlags.HardDependency)]

    public class LethalStats : BaseUnityPlugin
    {

        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        private static LethalStats Instance;
        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }

            harmony.PatchAll();
           
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
            Logger.LogInfo($"{PluginInfo.PLUGIN_NAME} will upload stats when you see your post-game summary screen!");
        }





    }
}