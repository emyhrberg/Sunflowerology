using System;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace Sunflowerology.Common.Configs
{
    public class Config : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("JamOfBiomology")]

        [DefaultValue(30)]
        public int PlantUpdateInterval = 30;
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