using System;
using ScienceJam.Content.Tiles;
using ScienceJam.Content.Tiles.SunGrass;
using ScienceJam.Content.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Projectiles
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

                        if (wall != 0 && wall != ModContent.WallType<SungrassWallUnsafe>())
                        {
                            if (wall == WallID.SpiderUnsafe)
                                Main.tile[k, l].WallType = (ushort)ModContent.WallType<SungrassWallUnsafe>();
                            else
                                Main.tile[k, l].WallType = (ushort)ModContent.WallType<SungrassWall>();
                            WorldGen.SquareWallFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }

                        if (TileID.Sets.Conversion.Grass[type] && type != ModContent.TileType<SunGrassTile>())
                        {
                            Main.tile[k, l].TileType = (ushort)ModContent.TileType<SunGrassTile>();
                            ModContent.GetInstance<SunGrassTileEntity>().Place(k, l);
                            WorldGen.TileFrame(k, l);
                            WorldGen.DiamondTileFrame(k, l);
                            NetMessage.SendTileSquare(-1, k, l, 1);
                        }
                    }
                }
            }
        }
    }
}
