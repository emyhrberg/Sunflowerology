using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.SunGrass
{
    internal class SunGrassLargeFoliage : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.CoordinateHeights = new int[]
            {
                32
            };
            TileObjectData.newTile.DrawYOffset = -10;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 8;
            TileObjectData.newTile.StyleMultiplier = 1;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            Main.tileSolid[Type] = false;
            Main.tileCut[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileWaterDeath[Type] = true;
            TileID.Sets.SwaysInWindBasic[Type] = true;
            TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
            TileID.Sets.TileCutIgnore.Regrowth[Type] = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorValidTiles = new int[]
            {
                ModContent.TileType<SunGrassTile>()
            };
            TileObjectData.addTile(Type);
            HitSound = new SoundStyle?(SoundID.Grass);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile tile = Main.tile[i, j];
            tile.TileFrameX %= 18 * 8; // 18 pixels per frame, 8 frames total
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }

        public override void PostTileFrame(int i, int j, int up, int down, int left, int right, int upLeft, int upRight, int downLeft, int downRight)
        {
            base.PostTileFrame(i, j, up, down, left, right, upLeft, upRight, downLeft, downRight);
        }
    }
}
