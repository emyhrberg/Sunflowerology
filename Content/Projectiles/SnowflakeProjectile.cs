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
	internal class SnowflakeProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NorthPoleSnowflake;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.NorthPoleSnowflake);
			Projectile.aiStyle = -1; // Custom AI
			Projectile.melee = false;
			Projectile.penetrate = 1;
			Projectile.scale = 1f;
			Projectile.alpha = 0;
			Projectile.timeLeft = 120;
		}

		public override void AI()
		{
			Projectile.frame = (int)Projectile.ai[0];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity *= 0.98f; // Gradually slow down the projectile
			if (Projectile.velocity.Length() < 0.05f)
			{
				Projectile.velocity = Vector2.Zero;
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				int dustIndex = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.SnowflakeIce, 0f, 0f, 50);
				Dust dust = Main.dust[dustIndex];
				dust.noGravity = true;
				dust.velocity.X *= 0.75f;
				dust.velocity.Y *= 0.75f;
				dust.velocity -= Projectile.velocity * 0.025f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
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
			float timerFactor = MathHelper.Min(60f, 120 - Projectile.timeLeft) / 2f;

			// Draw multiple fading instances to create a trail effect
			for (float fade = 1f; fade >= 0f; fade -= 0.25f)
			{
				// Position offset for this trail instance
				Vector2 drawOffset = fade * (Projectile.velocity * 0.33f) * timerFactor;
				Vector2 drawPosition = Projectile.position - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY) - drawOffset;

				// Adjust color and scale based on how far back the trail instance is
				Color trailColor = baseColor * (1f - fade) * 0.75f;
				float trailScale = Projectile.scale * (0.4f + (1f - fade) * 0.6f);

				Main.EntitySpriteDraw(
					texture,
					drawPosition,
					sourceRectangle,
					trailColor,
					Projectile.rotation,
					origin,
					trailScale,
					spriteEffects);
			}
			return false;
		}
	}
}
