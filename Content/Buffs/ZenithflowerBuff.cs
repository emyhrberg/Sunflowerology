using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace Sunflowerology.Content.Buffs
{
    internal class ZenithflowerBuff : ModBuff
    {
        public static readonly float projTagMultiplier = 9f;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;  // hide the ticking timer
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.allDamage.Multiplicative += 10;
        }
    }

    public class ZenithflowerBuffNPC : GlobalNPC
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // Only player attacks should benefit from this buff, hence the NPC and trap checks.
            if (projectile.npcProj || projectile.trap)
                return;

            if (npc.HasBuff<ZenithflowerBuff>())
            {
                // Apply the scaling bonus to the next hit, and then remove the buff, like the vanilla firecracker
                modifiers.ScalingBonusDamage += 9 * ZenithflowerBuff.projTagMultiplier;
                npc.RequestBuffRemoval(ModContent.BuffType<ZenithflowerBuff>());
            }
        }
    }
}
