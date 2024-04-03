using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Text;

namespace LethalStats.Models
{
    //This is used to store the mods that are currently loaded, I will never reveal this information to anyone, it is used for the new daily challenge system so I can make mod specific challenges.
    public class DanosPlayerMods
    {
        public static List<DanosPlayerMods> CollectMods()
        {
            List<DanosPlayerMods> mods = new List<DanosPlayerMods>();
            try
            {
                var pluginsDict = Chainloader.PluginInfos;
                foreach (var plugin in pluginsDict.Values)
                {
                    mods.Add(new DanosPlayerMods
                    {
                        ModGUID = plugin.Metadata.GUID,
                        ModName = plugin.Metadata.Name,
                        ModVersion = plugin.Metadata.Version.ToString()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CollectMods: {ex.Message}");
                mods = new List<DanosPlayerMods>();
            }
            return mods;
        }
        public string ModGUID { get; set; } = "";
        public string ModName { get; set; } = "";
        public string ModVersion { get; set; } = "";

    }
}
