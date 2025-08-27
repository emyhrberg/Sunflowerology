using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace Sunflowerology.Content.Buffs
{
    internal class DeadSunflowerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;  // hide the ticking timer
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statLifeMax2 -= player.statLifeMax2/2;             // flat bonus, stacks with other sources
        }
    }
}
