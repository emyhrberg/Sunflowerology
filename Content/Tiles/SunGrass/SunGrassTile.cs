using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Common.Configs;
using ScienceJam.Content.Biomes;
using ScienceJam.Content.Dusts;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.SunGrass
{
    public class SunGrassTile : ModTile
    {
        public static Asset<Texture2D> glowTexture;
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
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            // This is the important line!
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<SunGrassTileEntity>().Generic_HookPostPlaceMyPlayer;

            TileObjectData.addTile(Type);
            DustType = ModContent.DustType<SunGrassSparkle>();

            AddMapEntry(new Color(200, 200, 200));

            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
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
                ModContent.GetInstance<SunGrassTileEntity>().Kill(i, j);
                WorldGen.SquareTileFrame(i, j);
                NetMessage.SendTileSquare(-1, i, j, 1);
                SoundEngine.PlaySound(SoundID.Grass);
                for (int k = 0; k < 10; k++)
                {
                    int dust = Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustType);
                }

            }
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

            Tile upperTile = Framing.GetTileSafely(i, j - 1);

            if (!upperTile.HasTile && Main.tile[i, j].HasTile && Utils.NextBool(Main.rand, 4) && upperTile.LiquidAmount == 0)
            {
                WorldGen.PlaceObject(i, j - 1, ModContent.TileType<SunGrassSmallFoliage>(), true, 0, 0, -1, -1);
                WorldGen.TileFrame(i, j - 1);
            }
            else if (upperTile.HasTile && Main.tile[i, j].HasTile && upperTile.type == ModContent.TileType<SunGrassSmallFoliage>() && Utils.NextBool(Main.rand, 4))
            {
                upperTile.type = (ushort)ModContent.TileType<SunGrassLargeFoliage>();
                WorldGen.TileFrame(i, j - 1);
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
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            SJUtils.DrawGlowmask(i, j);
        }
    }
    public class SunGrassTileEntity : ModTileEntity
    {
        int counter = 0;
        bool needCheckForSunflower => counter == 0;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SunGrassTile>();
        }

        public override void Update()
        {
            if(!IsTileValidForEntity(Position.X, Position.Y))
            {
                Kill(Position.X, Position.Y);
            }

            if (!WorldGen.InWorld(Position.X, Position.Y, 10))
            {
                return;
            }

            counter++;
            counter %= Conf.C.HowFastSunGrassDecays;
            if (!needCheckForSunflower || !Main.rand.NextBool(10))
            {
                return;
            }
            counter = 0;
            int i = Position.X;
            int j = Position.Y;
            int radius = Conf.C.RadiusOfSunflower;

            bool sunflowerNearby = false;

            for (int k = -radius; k <= radius && !sunflowerNearby; k++)
            {
                for (int l = -radius; l <= radius && !sunflowerNearby; l++)
                {
                    if (k * k + l * l <= radius * radius && WorldGen.InWorld(i + k, j + l, 10))
                    {
                        Tile tile = Framing.GetTileSafely(i + k, j + l);
                        if (tile.HasTile && tile.TileType == TileID.Sunflower)
                        {
                            sunflowerNearby = true;
                        }
                    }
                }
            }

            if (!sunflowerNearby)
            {
                Main.tile[i, j].type = TileID.Grass;
                WorldGen.SquareTileFrame(i, j);
                Kill(i, j);
                return;
            }
        }
    }
}