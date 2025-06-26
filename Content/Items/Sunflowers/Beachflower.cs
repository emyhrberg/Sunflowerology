using ScienceJam.Content.Tiles.DeadSunflower;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Sunflowers
{
    internal class Beachflower : FlowerItem
    {
        protected override int SunflowerItemId => ModContent.TileType<BeachflowerTile>();
    }
}
