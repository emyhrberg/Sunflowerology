using CotlimsCoolMod.Backgrounds;
using CotlimsCoolMod.Common.Systems;
using CotlimsCoolMod.Content.Items.Placeable;
using Microsoft.Xna.Framework;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CotlimsCoolMod.Content.Biomes
{
    // Shows setting up two basic biomes. For a more complicated example, please request.
    public class DaylandSurfaceBiome : ModBiome
    {
        // Select all the scenery
        public override ModWaterStyle WaterStyle => ModContent.GetInstance<DaylandWaterStyle>(); // Sets a water style for when inside this biome
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<DaylandSurfaceBackgroundStyle>();
        public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

        // Select Music 
        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery");

        public override int BiomeTorchItemType => ModContent.ItemType<SunGrassTorch>();
        public override int BiomeCampfireItemType => ModContent.ItemType<SunGrassCampfire>();

        // Populate the Bestiary Filter
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath; // Re-uses Bestiary Background for Map Background

        // Calculate when the biome is active.
        public override bool IsBiomeActive(Player player)
        {
            // First, we will use the exampleBlockCount from our added ModSystem for our first custom condition
            bool b1 = SunGrassTileCount.SunGrassTileInfluence >= 1;

            // Second, we will limit this biome to the inner horizontal third of the map as our second custom condition
            bool b2 = Math.Abs(player.position.ToTileCoordinates().X - Main.maxTilesX / 2) < Main.maxTilesX / 6;

            // Finally, we will limit the height at which this biome can be active to above ground (ie sky and surface). Most (if not all) surface biomes will use this condition.
            bool b3 = player.ZoneSkyHeight || player.ZoneOverworldHeight;
            return b1 && b3;
        }

        public override void MapBackgroundColor(ref Color color)
        {
            base.MapBackgroundColor(ref color);
        }


        public override void OnInBiome(Player player)
        {
            base.OnInBiome(player);
            
        }
        // Declare biome priority. The default is BiomeLow so this is only necessary if it needs a higher priority.
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

        public override void Load()
        {
            base.Load();
            
        }

        public static void SetCotlandCollor(On_Main.orig_SetBackColor orig, Main.InfoToSetBackColor info, out Color sunColor, out Color moonColor)
        {
            float influence = SunGrassTileCount.SunGrassTileInfluence;
            //Main.NewText(influence);
            orig(info, out sunColor, out moonColor);
            if (influence > 0.01f && !Main.dayTime)
            {
                
                double time = Main.time;
                
                bool dayTime = Main.dayTime;
                
                Main.time = Main.time / Main.nightLength * Main.dayLength;
                Main.dayTime = true;
                
                orig(info, out sunColor, out moonColor);
                Color biomeTimeColor = new Color(Main.ColorOfTheSkies.ToVector4());
                biomeTimeColor.B = (byte)((float)biomeTimeColor.B * 0.8f);
                Main.time = time;
                Main.dayTime = dayTime;

                orig(info, out sunColor, out moonColor);
                moonColor = new Color((sunColor.ToVector4()* (1f - influence) + (sunColor.ToVector4() + Main.DiscoColor.ToVector4()) * influence / 2f));
                sunColor = moonColor;
                Main.ColorOfTheSkies = new Color(biomeTimeColor.ToVector4() * influence + Main.ColorOfTheSkies.ToVector4() * (1f - influence));

            }
            


            //Main.NewText($"{((float)Main.ColorOfTheSkies.R + ((float)Main.DiscoColor.R / 255f * range - range / 2f))}");
        }
    }
}
