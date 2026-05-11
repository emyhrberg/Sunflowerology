using Sunflowerology.Common.Systems;
using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Projectiles;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Common.GlobalProjectiles
{
    internal class IceflowerProjBonus : GlobalProjectile
    {

        public override void OnKill(Projectile projectile, int timeLeft)
        {
            TrySpawnShockwave(projectile);
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrySpawnShockwave(projectile);
        }

        private static void TrySpawnShockwave(Projectile projectile)
        {
            if (IceflowerProjBonusCounter.ShockwaveSpawnCooldown > 0 || projectile.type == ModContent.ProjectileType<ShockwaveProjectile>())
                return;

            Player player = Main.player[projectile.owner];
            if (projectile.damage <= 0 || projectile.hostile || player.ownedProjectileCounts[ModContent.ProjectileType<ShockwaveProjectile>()] > 5 || (player.position - projectile.Center).Length() > 16 * 70)
                return;

            IceflowerProjBonusCounter.ResetCounter();

            if (projectile.owner == Main.myPlayer &&
                player.HasBuff<IceflowerBuff>() &&
                !player.dead &&
                player.active &&
                !player.ghost)
            {
                Projectile.NewProjectile(projectile.GetSource_FromThis(), projectile.Center,
                    Vector2.Zero, ModContent.ProjectileType<ShockwaveProjectile>(),
                    (int)Math.Ceiling(projectile.damage / 10f), 0, projectile.owner);
            }
        }
    }

    internal class IceflowerProjBonusMele : GlobalItem
    {
        public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            TrySpawnShockwave(player, target, hit);
        }
        private void TrySpawnShockwave(Player player, NPC target, NPC.HitInfo hit)
        {
            if (IceflowerProjBonusCounter.ShockwaveSpawnCooldown > 0)
                return;
            if (player.ownedProjectileCounts[ModContent.ProjectileType<ShockwaveProjectile>()] > 50 || (player.position - target.Center).Length() > 16 * 70)
                return;
            IceflowerProjBonusCounter.ResetCounter();
            if (player.HasBuff<IceflowerBuff>() &&
                !player.dead &&
                player.active &&
                !player.ghost)
            {
                Projectile.NewProjectile(player.GetSource_FromThis(), target.Center,
                    Vector2.Zero, ModContent.ProjectileType<ShockwaveProjectile>(),
                    (int)Math.Ceiling(hit.Damage / 10f), 0, player.whoAmI);
            }
        }
    }
}
