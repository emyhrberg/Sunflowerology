using System;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ScienceJam.Common.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("JamOfBiomology")]

        [DefaultValue(true)]
        public bool ShowMessageWhenEnteringWorld;

        [DefaultValue(true)]
        public bool DestabilizeTiles = true;

        [DefaultValue(3)]
        public int RadiusOfSunflower = 3;

        [DefaultValue(25)]
        public int HowFastSunGrassDecays = 25;

        [DefaultValue(60f)]
        public int HowMuchYouNeedBlocksForDayland = 60;
    }


    public static class Conf
    {
        public static void Save()
        {
            try
            {
                ConfigManager.Save(C);
            }
            catch
            {
                Log.Error("An error occurred while manually saving ModConfig!.");
            }
        }

        // Instance of the Config class
        // Use it like 'Conf.C.YourConfigField' for easy access to the config values
        public static Config C
        {
            get
            {
                try
                {
                    return ModContent.GetInstance<Config>();
                }
                catch (Exception ex)
                {
                    Log.Error("Error getting config instance: " + ex.Message);
                    return null;
                }
            }
        }
    }
}