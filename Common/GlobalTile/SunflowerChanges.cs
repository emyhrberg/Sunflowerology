using ScienceJam.Content.Tiles.SunGrass;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace SienceJam.Common.GlobalTiles
{
	public class SunflowerChanges : GlobalTile
	{
		public override void SetStaticDefaults() {
			TileObjectData tileObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
			tileObjectData.AnchorValidTiles = tileObjectData.AnchorValidTiles.Append(ModContent.TileType<SunGrassTile>()).ToArray();
		}

		public override void Unload() {
			TileObjectData tileObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
			tileObjectData.AnchorValidTiles = tileObjectData.AnchorValidTiles.Except([ModContent.TileType<SunGrassTile>()]).ToArray();
		}
	}
}