using ScienceJam.Backgrounds;
using ScienceJam.Common.Systems;
using ScienceJam.Content.Items.Placeable;
using System;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace ScienceJam.Content.Biomes
{
    public class DaylandSurfaceBiome : ModBiome
    {
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<DaylandSurfaceBackgroundStyle>();
        public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

        public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/MysteriousMystery");

        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => base.BackgroundColor;
        public override string MapBackground => BackgroundPath;

        public override bool IsBiomeActive(Player player)
        {
            bool b1 = SunGrassTileCount.SunGrassTileInfluence >= 0.8f;
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
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;

        public override void Load()
        {
            base.Load();

        }
    }
}
