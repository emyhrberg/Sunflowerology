using Microsoft.Xna.Framework;
using System;
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
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Terraria.ID.BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statLifeMax2 -= (int)Math.Floor((float)player.statLifeMax2 / 2.0);
        }
    }
}
