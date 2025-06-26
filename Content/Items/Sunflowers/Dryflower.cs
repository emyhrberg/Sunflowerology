using ScienceJam.Content.Tiles.DeadSunflower;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Sunflowers
{
    internal class Dryflower : FlowerItem
    {
        protected override int SunflowerItemId => ModContent.TileType<DryflowerTile>();
    }
}
