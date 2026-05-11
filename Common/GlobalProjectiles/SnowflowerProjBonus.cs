using Microsoft.Xna.Framework;
using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Common.GlobalProjectiles
{
    internal class SnowflowerProjBonus : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        private const int SNOWFLAKE_MIN_DISTANCE = 16*20;

        public Vector2? posLastSnowflake;
        public override void PostAI(Projectile projectile)
        {

            if(projectile.type == ModContent.ProjectileType<SnowflakeProjectile>())
                return;

            if (!posLastSnowflake.HasValue)
            {
                posLastSnowflake = projectile.Center;
                return;
            }

            if (posLastSnowflake.HasValue &&
                (posLastSnowflake.Value - projectile.Center).Length() > SNOWFLAKE_MIN_DISTANCE)
            {
                TrySpawnTrail(projectile);
            }

        }

        private void TrySpawnTrail(Projectile projectile)
        {
            Player player = Main.player[projectile.owner];
            if (projectile.damage <= 0 || player.ownedProjectileCounts[projectile.type] > 50 || (player.position - projectile.Center).Length() > 16*70)
                return;
            float distance = (projectile.Center - posLastSnowflake.Value).Length();
            int count = Math.Min(10, (int)(distance / SNOWFLAKE_MIN_DISTANCE));
            float step = distance / count;
            for (int i = 1; i <= count; i++)
            {
                var newPos = Vector2.Normalize(projectile.Center - posLastSnowflake.Value) * step + posLastSnowflake.Value;
                if (projectile.owner == Main.myPlayer &&
                    player.HasBuff<SnowflowerBuff>() &&
                    !player.dead &&
                    player.active &&
                    !player.ghost)
                {
                    for (int j = 0; j <= 5; j++)
                    {
                        var p = Projectile.NewProjectile(projectile.GetSource_FromThis(), newPos,
                                Vector2.Normalize(projectile.velocity).RotatedBy(MathHelper.TwoPi / 6 * j) * 2, ModContent.ProjectileType<SnowflakeProjectile>(),
                                (int)Math.Ceiling(projectile.damage / 30f), 0, projectile.owner, 0f, Main.rand.Next(3));
                    }
                }
                posLastSnowflake = newPos;
            }
        }
    }
}
