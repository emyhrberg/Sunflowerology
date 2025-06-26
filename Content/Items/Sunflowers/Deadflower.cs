using ScienceJam.Content.Tiles.DeadSunflower;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Sunflowers
{
    internal class Deadflower : FlowerItem
    {
        protected override int SunflowerItemId => ModContent.TileType<DeadflowerTile>();
    }
}
