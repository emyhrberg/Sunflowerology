using ScienceJam.Content.Biomes;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam.Common.Systems
{
    internal class CustomSkyColorSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            On_Main.SetBackColor += DaylandSurfaceBiome.SetCotlandCollor;
        }
        public override void OnWorldUnload()
        {
            On_Main.SetBackColor -= DaylandSurfaceBiome.SetCotlandCollor;
        }

    }
}
