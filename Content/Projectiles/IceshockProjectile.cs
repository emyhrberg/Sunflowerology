using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Projectiles
{
	internal class IceshockProjectile : ModProjectile
	{
		private const float LifetimeTicks = 60f;
		private const float VelocityOffset = 30f;
		private const float VelocityScale = 0.25f;
		private const float ArcTurnRadians = MathHelper.TwoPi / 6f;

		private Vector2 _frameMovement;
		private Vector2 _spawnPosition;
		private bool _spawnPositionInitialized;

		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NorthPoleSnowflake;

		public float Direction
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		public float Strength
		{
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		private float GetTurnOffset(float elapsedTicks)
		{
			float progress = MathHelper.Clamp(elapsedTicks / LifetimeTicks, 0f, 1f);
			float smoothProgress = MathHelper.SmoothStep(0f, 1f, progress);
			return ArcTurnRadians * smoothProgress;
		}

		private Vector2 GetDirectionVector(float elapsedTicks)
		{
			return Vector2.One.RotatedBy(Direction + GetTurnOffset(elapsedTicks)) * Strength / 3;
		}

		private float GetElapsedTicks()
		{
			return LifetimeTicks - Projectile.timeLeft;
		}

		private float GetIntegratedDistance(float elapsedTicks)
		{
			float clampedElapsedTicks = MathHelper.Clamp(elapsedTicks, 0f, LifetimeTicks);
			return VelocityScale * (VelocityOffset * clampedElapsedTicks - (clampedElapsedTicks * (clampedElapsedTicks - 1f)) * 0.5f);
		}

		private Vector2 GetPositionFromFormula(float elapsedTicks)
		{
			return _spawnPosition + GetDirectionVector(elapsedTicks) * GetIntegratedDistance(elapsedTicks);
		}

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.NorthPoleSnowflake);
			Projectile.aiStyle = -1; // Custom AI
			Projectile.melee = false;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;
			Projectile.timeLeft = 60;
		}

		public override void AI()
		{
			if (!_spawnPositionInitialized)
			{
				_spawnPosition = Projectile.position;
				_spawnPositionInitialized = true;
			}

			Projectile.frame = (int)Projectile.ai[0];
			float elapsedTicks = GetElapsedTicks();
			Vector2 currentPosition = GetPositionFromFormula(elapsedTicks);
			Vector2 previousPosition = GetPositionFromFormula(elapsedTicks - 1f);
			_frameMovement = currentPosition - previousPosition;
			Projectile.position = currentPosition;
			Projectile.rotation = MathHelper.TwoPi / LifetimeTicks * elapsedTicks * 3;
			Projectile.velocity = Vector2.Zero;
			Projectile.alpha = (int)(255f * Math.Max(elapsedTicks / LifetimeTicks - 0.75f, 0f) * 4f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (!_spawnPositionInitialized)
			{
				_spawnPosition = Projectile.position;
				_spawnPositionInitialized = true;
			}

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int textureWidth = TextureAssets.Projectile[Type].Width();

			// Texture framing
			int frameHeight = TextureAssets.Projectile[Type].Height() / Main.projFrames[Type];
			int frameY = frameHeight * Projectile.frame;
			Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, frameY, textureWidth, frameHeight - 1);

			// Calculate projectile opacity and base color
			float opacity = 1f - Projectile.alpha / 255f;
			Color baseColor = new Color((int)(250f * opacity), (int)(250f * opacity), (int)(250f * opacity), (int)(100f * opacity));

			// Calculate the origin point of the texture
			float originX = (float)(textureWidth - Projectile.width) * 0.5f + (float)Projectile.width * 0.5f;
			Vector2 origin = new Vector2(originX, Projectile.height / 2f);

			// Flip sprite depending on projectile direction
			SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			// Calculate the time factor for the trail length
			float elapsedTicks = GetElapsedTicks();
			

			// Draw multiple fading instances to create a trail effect
			for (float fade = 1f; fade >= 0f; fade -= 0.25f)
			{
				float timerFactor = MathHelper.Min(LifetimeTicks, elapsedTicks) / 2;
				float trailTicksBack = fade * 4 * timerFactor * (LifetimeTicks / 2 - timerFactor) / (LifetimeTicks / 2);
				float sampledElapsedTicks = Math.Max(0f, elapsedTicks - trailTicksBack);
				Vector2 sampledPosition = GetPositionFromFormula(sampledElapsedTicks);
				Vector2 drawPosition = sampledPosition - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY);
				float sampledRotation = Projectile.rotation;

				// Adjust color and scale based on how far back the trail instance is
				Color trailColor = baseColor * (1f - fade) * 0.75f;
				float trailScale = Projectile.scale * (0.4f + (1f - fade) * 0.6f);

				Main.EntitySpriteDraw(
					texture,
					drawPosition,
					sourceRectangle,
					trailColor,
					sampledRotation,
					origin,
					trailScale,
					spriteEffects);
			}
			return false;
		}
	}
}
