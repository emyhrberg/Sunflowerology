using MonoMod.RuntimeDetour;
using Sunflowerology.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Systems
{
    internal class ExtraJumpSpeedDryflowerBonusHook : ModSystem
    {
        Hook hook;
        public override void Load()
        {
            hook = new Hook(
                typeof(ExtraJumpLoader).GetMethod(nameof(ExtraJumpLoader.UpdateHorizontalSpeeds), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public),
                typeof(ExtraJumpSpeedDryflowerBonusHook).GetMethod(nameof(UpdateHorizontalSpeedsHook), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            );
        }

        public override void Unload()
        {
            hook?.Dispose();
        }

        private static void UpdateHorizontalSpeedsHook(Action<Player> orig, Player player)
        {
            orig(player);

            if (!player.HasBuff(ModContent.BuffType<DryflowerBuff>()))
            {
                return;
            }

            foreach (ExtraJump moddedExtraJump in ExtraJumpLoader.orderedJumps)
            {
                ref ExtraJumpState extraJump = ref player.GetJumpState(moddedExtraJump);
                if (extraJump.Active)
                {
                    player.maxRunSpeed *= 1.25f;
                    return;
                }
            }
        }
    }
}
