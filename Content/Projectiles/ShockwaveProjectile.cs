using Microsoft.Xna.Framework;
using Sunflowerology.Common.Shaders;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Projectiles
{
    public class ShockwaveProjectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_0";

        

        private const int MaxTime = 60;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxTime;
            Projectile.alpha = 255;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 8;
        }

        public override void AI()
        {
            float progress = 1f - ((float)Projectile.timeLeft / MaxTime);

            Projectile.scale = 1f + progress * 10f;
            int newSize = (int)(20 * Projectile.scale);
            Vector2 oldCenter = Projectile.Center;
            Projectile.width = newSize;
            Projectile.height = newSize;
            Projectile.Center = oldCenter;

            if (Main.netMode == NetmodeID.Server || Projectile.owner != Main.myPlayer)
                return;

            Filter filter = Filters.Scene[ShockwaveShaderData.ShockwaveSceneFilterName];
            if (filter == null)
                return;

            if (!filter.IsActive())
            {
                Filters.Scene.Activate(ShockwaveShaderData.ShockwaveSceneFilterName, Projectile.Center)
                    .GetShader()
                    .UseOpacity(1f);
            }

            if (filter.GetShader() is ShockwaveShaderData shockwave)
            {
                shockwave.WaveCentre = Projectile.Center - Main.screenPosition;
                shockwave.UseProgress(MathHelper.Clamp(progress, 0f, 1f));
                shockwave.UseOpacity(MathHelper.Clamp(1f - progress, 0f, 0.25f) / 0.25f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Filter filter = Filters.Scene[ShockwaveShaderData.ShockwaveSceneFilterName];
            if (filter?.IsActive() == true)
                filter.Deactivate();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Only owner of the projectile can see the shockwave, so would be unfair if it damaged other players if they dont see it.
            info.Damage = 0;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float radius = Projectile.width / 2f;
            Vector2 center = Projectile.Center;

            float closestX = MathHelper.Clamp(center.X, targetHitbox.Left, targetHitbox.Right);
            float closestY = MathHelper.Clamp(center.Y, targetHitbox.Top, targetHitbox.Bottom);

            float dx = center.X - closestX;
            float dy = center.Y - closestY;

            return (dx * dx + dy * dy) < (radius * radius);
        }
    }
}
