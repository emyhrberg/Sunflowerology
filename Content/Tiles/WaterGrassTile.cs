using ScienceJam.Content.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles
{
    public class WaterGrassTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMerge[Type][0] = true;
            Main.tileMerge[0][Type] = true;
            Main.tileMerge[Type][2] = true;
            Main.tileMerge[2][Type] = true;
            Main.tileMerge[Type][23] = true;
            Main.tileMerge[23][Type] = true;
            Main.tileMerge[Type][199] = true;
            Main.tileMerge[199][Type] = true;
            Main.tileMerge[Type][109] = true;
            Main.tileMerge[109][Type] = true;
            Main.tileMerge[TileID.Sand][Type] = true;
            Main.tileMerge[Type][TileID.Sand] = true;
            //Main.tileBlendAll[Type] = true;
            Main.tileBlockLight[Type] = true;
            TileID.Sets.Conversion.Grass[Type] = true;
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
            TileID.Sets.SpreadOverground[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;
            TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            TileID.Sets.NeedsGrassFraming[Type] = true;
            TileID.Sets.NeedsGrassFramingDirt[Type] = TileID.Sand;


            DustType = ModContent.DustType<WaterGrassSparkle>();

            AddMapEntry(new Color(200, 200, 200));

        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!effectOnly)
            {
                fail = true;
                effectOnly = true;
                Framing.GetTileSafely(i, j).TileType = TileID.Sand;
                WorldGen.SquareTileFrame(i, j);
                NetMessage.SendTileSquare(-1, i, j, 1);
                SoundEngine.PlaySound(SoundID.Grass);
                for (int k = 0; k < 10; k++)
                {
                    int dust = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType);
                }

            }
        }

        public unsafe override void RandomUpdate(int i, int j)
        {
            if (!WorldGen.InWorld(i, j, 10))
            {
                return;
            }
            Tile downerTile = Framing.GetTileSafely(i, j + 1);
            Tile upperTile = Framing.GetTileSafely(i, j - 1);

            if (!upperTile.HasTile && Main.tile[i, j].HasTile && Utils.NextBool(Main.rand, 4) && upperTile.LiquidAmount == 0)
            {
                WorldGen.PlaceObject(i, j - 1, ModContent.TileType<WaterGrassSmallFoliage>(), true, Main.rand.Next(22), 0, -1, -1);
            }

            if (!downerTile.HasTile || !(downerTile.TileType == TileID.Sand || downerTile.TileType == Type))
            {
                Framing.GetTileSafely(i, j).TileType = (ushort)TileID.Sand;
                WorldGen.SquareTileFrame(i, j);
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile upperTile = Framing.GetTileSafely(i, j - 1);
            if (TileID.Sets.TileCutIgnore.Regrowth[upperTile.TileType] && upperTile.HasTile && upperTile.TileType != (ushort)ModContent.TileType<SunGrassSmallFoliage>())
            {
                upperTile.TileType = (ushort)ModContent.TileType<WaterGrassSmallFoliage>();
                WorldGen.TileFrame(i, j - 1);
                NetMessage.SendTileSquare(-1, i, j - 1, 1);
            }
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }

        public override bool CanReplace(int i, int j, int tileTypeBeingPlaced)
        {
            if (tileTypeBeingPlaced == TileID.Sand)
            {
                return false;
            }
            else
            {
                bool _ = false;
                KillTile(i, j, ref _, ref _, ref _);
                return true;
            }
        }
    }
}