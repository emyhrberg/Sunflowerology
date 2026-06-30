using Microsoft.Xna.Framework;
using Sunflowerology.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace Sunflowerology.Content.Buffs
{
    internal class FireflowerBuff : ModBuff
    {
        static Item fireflowerDummyItem;
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            fireflowerDummyItem = new Item(ModContent.ItemType<FireflowerWings>());
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffImmune[BuffID.Suffocation] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[ModContent.BuffType<SnowflowerBuff>()] = true;
            player.buffImmune[ModContent.BuffType<IceflowerBuff>()] = true;

            player.ApplyEquipFunctional(fireflowerDummyItem, false);

        }
    }
}

