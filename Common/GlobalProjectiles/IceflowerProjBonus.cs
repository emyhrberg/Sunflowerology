using Microsoft.Xna.Framework;
using Sunflowerology.Common.Systems;
using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Sunflowerology.Common.GlobalProjectiles
{
	internal static class IceflowerShockwaveSpawner
	{
		public static bool CanSpawn(Player player, Vector2 hitPosition, int maxOwnedShockwaves)
		{
			return player.HasBuff<IceflowerBuff>() &&
				   !player.dead &&
				   player.active &&
				   !player.ghost &&
				   player.ownedProjectileCounts[ModContent.ProjectileType<ShockwaveProjectile>()] <= maxOwnedShockwaves &&
				   (player.position - hitPosition).Length() <= 16 * 70;
		}

		public static void SpawnBurst(IEntitySource source, Player player, Vector2 hitPosition, int owner, int baseDamage)
		{
			Projectile.NewProjectile(
				source,
				hitPosition,
				Vector2.Zero,
				ModContent.ProjectileType<ShockwaveProjectile>(),
				(int)Math.Ceiling(baseDamage / 10f),
				0f,
				owner);

			int iceShockDamage = player.HasBuff<SnowflowerBuff>() ? (int)Math.Ceiling(baseDamage / 30f) : 0;
			float ai2 = player.HasBuff<SnowflowerBuff>() ? 3f : 2f;

			for (int j = 0; j <= 5; j++)
			{
				Projectile.NewProjectile(
					source,
					hitPosition,
					Vector2.Zero,
					ModContent.ProjectileType<IceshockProjectile>(),
					iceShockDamage,
					0f,
					owner,
					Main.rand.Next(3),
					MathHelper.TwoPi / 6 * j,
					ai2);
			}
		}
	}

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
			if (IceflowerProjBonusCounter.ShockwaveSpawnCooldown > 0 || 
				projectile.type == ModContent.ProjectileType<ShockwaveProjectile>() ||
				projectile.type == ModContent.ProjectileType<SnowflakeProjectile>() || 
				projectile.type == ModContent.ProjectileType<IceshockProjectile>())
				return;

			Player player = Main.player[projectile.owner];
			if (projectile.damage <= 0 || projectile.hostile)
				return;

			if (projectile.owner == Main.myPlayer &&
				IceflowerShockwaveSpawner.CanSpawn(player, projectile.Center, 5))
			{
				IceflowerProjBonusCounter.ResetCounter();
				IceflowerShockwaveSpawner.SpawnBurst(projectile.GetSource_FromThis(), player, projectile.Center, projectile.owner, projectile.damage);
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
			if (hit.Damage <= 0)
				return;

			if (player.whoAmI == Main.myPlayer &&
				IceflowerShockwaveSpawner.CanSpawn(player, target.Center, 50))
			{
				IceflowerProjBonusCounter.ResetCounter();
				IceflowerShockwaveSpawner.SpawnBurst(player.GetSource_FromThis(), player, target.Center, player.whoAmI, hit.Damage);
			}
		}
	}
}
