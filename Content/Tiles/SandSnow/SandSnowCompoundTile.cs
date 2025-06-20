
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using ScienceJam.Common.Configs;
using ScienceJam.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles.SandSnow
{
    public class SandSnowCompoundTile : ModTile
    {

        private Dictionary<(int, int), float> stabilityTable = new();
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            // Sand specific properties
            Main.tileSand[Type] = true;
            TileID.Sets.Conversion.Sand[Type] = true; // Allows Clentaminator solutions to convert this tile to their respective Sand tiles.
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true; // Allows Sandshark enemies to "swim" in this sand.
            TileID.Sets.CanBeDugByShovel[Type] = true;
            TileID.Sets.Falling[Type] = true;
            TileID.Sets.Suffocate[Type] = true;
            TileID.Sets.FallingBlockProjectile[Type] = new TileID.Sets.FallingBlockProjectileInfo(ModContent.ProjectileType<SandSnowCompoundBallFallingProjectile>(), 10); // Tells which falling projectile to spawn when the tile should fall.

            TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
            TileID.Sets.GeneralPlacementTiles[Type] = false;
            TileID.Sets.ChecksForMerge[Type] = true;

            MineResist = 1f; // Sand tile typically require half as many hits to mine.
            DustType = DustID.Stone;
            this.HitSound = SoundID.GetLegacyStyle(2, 48);
            AddMapEntry(new Color(150, 150, 150));
        }

        public override bool HasWalkDust()
        {
            return Main.rand.NextBool(3);
        }

        public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color)
        {
            dustType = DustID.Sand;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (!Conf.C.DestabilizeTiles)
            {
                return;
            }
            if (!stabilityTable.ContainsKey((i, j)))
            {
                stabilityTable.Add((i, j), 100f);
            }
            else
            {
                if (Main.rand.NextBool((int)Math.Round(stabilityTable[(i, j)])))
                {
                    //Main.NewText($"Stability: {stability}");
                    Tile tile = Framing.GetTileSafely(i, j);
                    tile.TileType = TileID.SnowBlock;
                    WorldGen.SquareTileFrame(i, j);
                    NetMessage.SendTileSquare(-1, i, j, 1);
                    SpawnSandBallInWalidPosition(i, j);


                }
                else
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            if (x == 0 && y == 0)
                            {
                                continue;
                            }
                            Tile tile = Framing.GetTileSafely(i + x, j + y);
                            if (tile.HasTile)
                            {
                                if (tile.TileType == Type)
                                {
                                    stabilityTable[(i, j)] += 0.1f;
                                }
                                else if (tile.TileType == TileID.SnowBlock || tile.TileType == TileID.Sand)
                                {
                                    stabilityTable[(i, j)] += 0.05f;
                                }
                            }
                        }
                    }
                    stabilityTable[(i, j)]--;
                }
            }

        }

        private void SpawnSandBallInWalidPosition(int i, int j)
        {
            bool rightSidePriority = Main.rand.NextBool(2);
            int offsetMultiplier = rightSidePriority ? -1 : 1;
            int[] xOffset = { 0, offsetMultiplier, -offsetMultiplier };
            for (int y = 1; y <= 30; y++)
            {
                foreach (int x in xOffset)
                {
                    if (!Framing.GetTileSafely(i + x, j - y).HasTile)
                    {
                        Projectile.NewProjectileDirect(default, new Vector2(i + x, j - y) * 16, Main.rand.NextVector2Unit(MathHelper.PiOver4 * (5 + (x / 2)), MathHelper.PiOver2) * 9,
                        ModContent.ProjectileType<SandBallFallingNoItemProjectile>(), 10, 0);
                        return;
                    }
                }
            }
            Projectile.NewProjectileDirect(default, new Vector2(i, j - 1) * 16, Main.rand.NextVector2Unit(MathHelper.PiOver4 * 5, MathHelper.PiOver2) * 9,
                        ModContent.ProjectileType<SandBallFallingNoItemProjectile>(), 10, 0);

        }
    }
}