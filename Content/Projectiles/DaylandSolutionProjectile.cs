using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using CotlimsCoolMod.Content.Walls;
using CotlimsCoolMod.Content.Tiles;

namespace CotlimsCoolMod.Content.Projectiles
{
    public class DaylandSolutionProjectile : ModProjectile
    {

        public ref float Progress => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            // This method quickly sets the projectile properties to match other sprays.
            Projectile.DefaultToSpray();
            Projectile.aiStyle = 0; // Here we set aiStyle back to 0 because we have custom AI code
        }

        public override bool? CanCutTiles()
        {
            return false;
        }

        public override void AI()
        {
            // Set the dust type to ExampleSolution
            int dustType = ModContent.DustType<Dusts.DaylandSolution>();

            if (Projectile.owner == Main.myPlayer)
            {
                Convert((int)(Projectile.position.X + Projectile.width * 0.5f) / 16, (int)(Projectile.position.Y + Projectile.height * 0.5f) / 16, 2);
            }

            if (Projectile.timeLeft > 133)
            {
                Projectile.timeLeft = 133;
            }

            if (Progress > 7f)
            {
                float dustScale = 1f;

                if (Progress == 8f)
                {
                    dustScale = 0.2f;
                }
                else if (Progress == 9f)
                {
                    dustScale = 0.4f;
                }
                else if (Progress == 10f)
                {
                    dustScale = 0.6f;
                }
                else if (Progress == 11f)
                {
                    dustScale = 0.8f;
                }

                Progress += 1f;


                var dust = Dust.NewDustDirect(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, dustType, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 100);

                dust.noGravity = true;
                dust.scale *= 1.75f;
                dust.velocity.X *= 2f;
                dust.velocity.Y *= 2f;
                dust.scale *= dustScale;
            }
            else
            {
                Progress += 1f;
            }

            Projectile.rotation += 0.3f * Projectile.direction;
        }

        private static void Convert(int i, int j, int size = 4)
        {
            for (int k = i - size; k <= i + size; k++)
            {
                for (int l = j - size; l <= j + size; l++)
                {
                    if (WorldGen.InWorld(k, l, 1) && Math.Abs(k - i) + Math.Abs(l - j) < Math.Sqrt(size * size + size * size))
                    {
                        int type = Main.tile[k, l].TileType;
                        int wall = Main.tile[k, l].WallType;

                        // Convert all walls to ExampleWall (or ExampleWallUnsafe for SpiderUnsafe)
                        if (wall != 0 && wall != ModContent.WallType<SungrassWallUnsafe>())
                        {
                            if (wall == WallID.SpiderUnsafe)
                                Main.tile[k, l].WallType = (ushort)ModContent.WallType<SungrassWallUnsafe>();
                            else
                                Main.tile[k, l].WallType = (ushort)ModContent.WallType<SungrassWall>();
                            WorldGen.SquareWallFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }

                        // If the tile is stone, convert to ExampleBlock
                        if (TileID.Sets.Conversion.Grass[type])
                        {
                            Main.tile[k, l].TileType = (ushort)ModContent.TileType<SunGrassTile>();
                            WorldGen.TileFrame(k, l);
                            WorldGen.DiamondTileFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                        // If the tile is sand, convert to ExampleSand
                        /*
                        else if (TileID.Sets.Conversion.Sand[type])
                        {
                            Main.tile[k, l].TileType = (ushort)ModContent.TileType<ExampleSand>();
                            WorldGen.SquareTileFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                        // If the tile is a chair, convert to ExampleChair
                        else if (type == TileID.Chairs && Main.tile[k, l - 1].TileType == TileID.Chairs)
                        {
                            Main.tile[k, l].TileType = (ushort)ModContent.TileType<ExampleChair>();
                            Main.tile[k, l - 1].TileType = (ushort)ModContent.TileType<ExampleChair>();
                            WorldGen.SquareTileFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                        // If the tile is a workbench, convert to ExampleWorkBench
                        else if (type == TileID.WorkBenches && Main.tile[k - 1, l].TileType == TileID.WorkBenches)
                        {
                            Main.tile[k, l].TileType = (ushort)ModContent.TileType<ExampleWorkbench>();
                            Main.tile[k - 1, l].TileType = (ushort)ModContent.TileType<ExampleWorkbench>();
                            WorldGen.SquareTileFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }*/
                    }
                }
            }
        }
    }
}
