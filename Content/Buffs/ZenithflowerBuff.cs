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
            Main.debuff[Type] = true; // player can't manually remove this buff
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.allDamage.Multiplicative += 10;
            player.buffImmune[ModContent.BuffType<DeadSunflowerBuff>()] = true;
        }
    }
}
