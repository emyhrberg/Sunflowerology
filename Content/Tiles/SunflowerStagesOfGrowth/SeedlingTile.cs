using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SeedlingTile : PlantStageTile<SeedlingEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 2;

        protected override bool IsSpecialHook => false;

        protected override bool HaveGlow => false;

        protected override int[] Heights => [16, 18];
    }
    public class SeedlingEntity : PlantStageEntity<SaplingEntity>
    {
        protected override int TileType => ModContent.TileType<SeedlingTile>();

        protected override int NextTileType => ModContent.TileType<SaplingTile>();
    }

}
