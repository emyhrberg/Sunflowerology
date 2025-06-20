using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Content.Biomes;
using ScienceJam.Content.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles.SunGrass
{
    public class SunGrassTile : ModTile
    {
        private Asset<Texture2D> glowTexture;
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
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
            Main.tileBlockLight[Type] = true;
            Main.tileBlendAll[Type] = true;
            TileID.Sets.Conversion.MergesWithDirtInASpecialWay[Type] = true;
            TileID.Sets.Conversion.Grass[Type] = true;
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.ResetsHalfBrickPlacementAttempt[Type] = true;
            TileID.Sets.SpreadOverground[Type] = true;
            TileID.Sets.CanBeDugByShovel[Type] = true;
            TileID.Sets.DoesntPlaceWithTileReplacement[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;

            DustType = ModContent.DustType<SunGrassSparkle>();

            AddMapEntry(new Color(200, 200, 200));

            //glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");

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
                Framing.GetTileSafely(i, j).TileType = TileID.Grass;
                WorldGen.SquareTileFrame(i, j);
                NetMessage.SendTileSquare(-1, i, j, 1);
                SoundEngine.PlaySound(SoundID.Grass);
                for (int k = 0; k < 10; k++)
                {
                    int dust = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType);
                }

            }
        }
        public override bool CanPlace(int i, int j)
        {
            //Main.NewText(Framing.GetTileSafely(i, j).TileType);
            //return Framing.GetTileSafely(i, j).HasTile;
            return true;
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            /*8if (Framing.GetTileSafely(i, j).TileType == 0)
            {
                Framing.GetTileSafely(i, j).ResetToType(Type);
            }*/

        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            /*
            Tile tile = Main.tile[i, j];

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            spriteBatch.Draw(
                glowTexture.Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            */
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            r = 0.4f;
            g = 0.4f;
            b = 0f;
        }

        public unsafe override void RandomUpdate(int i, int j)
        {
            if (!WorldGen.InWorld(i, j, 10))
            {
                return;
            }
            Framing.GetTileSafely(i, j + 1);
            Tile upperTile = Framing.GetTileSafely(i, j - 1);

            if (!upperTile.HasTile && Main.tile[i, j].HasTile && Utils.NextBool(Main.rand, 4) && upperTile.LiquidAmount == 0)
            {
                WorldGen.PlaceObject(i, j - 1, ModContent.TileType<SunGrassSmallFoliage>(), true, Main.rand.Next(22), 0, -1, -1);
            }
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            Tile upperTile = Framing.GetTileSafely(i, j - 1);
            if (TileID.Sets.TileCutIgnore.Regrowth[upperTile.TileType] && upperTile.HasTile && upperTile.TileType != (ushort)ModContent.TileType<SunGrassSmallFoliage>())
            {
                upperTile.TileType = (ushort)ModContent.TileType<SunGrassSmallFoliage>();
                WorldGen.TileFrame(i, j - 1);
                NetMessage.SendTileSquare(-1, i, j - 1, 1);
            }
            return base.TileFrame(i, j, ref resetFrame, ref noBreak);
        }

        public override bool CanReplace(int i, int j, int tileTypeBeingPlaced)
        {
            if (tileTypeBeingPlaced == TileID.Dirt)
            {
                return false;
            }
            else
            {
                bool _ = false;
                KillTile(i, j, ref _, ref _, ref _);
                return true;
            }
            return base.CanReplace(i, j, tileTypeBeingPlaced);
        }
    }
}