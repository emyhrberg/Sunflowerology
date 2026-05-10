using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Dusts
{
    public class TrueSunflowerKillTileDust : ModDust
    {
        public override void OnSpawn(Dust dust)
        {
            dust.velocity = Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(1f, 2f);
            dust.noGravity = true;
            dust.noLight = true;
            dust.frame = new Rectangle(Main.rand.Next(0, 58 - 8), Main.rand.Next(0, 128 - 8), 8, 8);
            dust.alpha = 0;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity = dust.velocity.RotatedBy((255f - dust.alpha) / 2550f / 2f);
            dust.alpha += 2;

            float light = 0.35f * (255f - dust.alpha) / 255f;

            Lighting.AddLight(dust.position, light, light, 0);

            if (dust.alpha >= 255)
            {
                dust.active = false;
            }

            return false;
        }
    }
}