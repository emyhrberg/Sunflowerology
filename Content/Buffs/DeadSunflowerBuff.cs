using Terraria;
using Terraria.ModLoader;

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
            player.statLifeMax2 -= 5;             // flat bonus, stacks with other sources
        }
    }
}
