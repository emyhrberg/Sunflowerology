using Mono.Cecil.Cil;
using MonoMod.Cil;
using Sunflowerology.Content.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Systems
{
    internal class BigTrueSunflowerBuffHook : ModSystem
    {

        public override void Load()
        {
            IL_Main.DrawInterface_Resources_Buffs += Main_DrawInterface_Resources_Buffs_Hook;
        }

        public override void Unload()
        {
            IL_Main.DrawInterface_Resources_Buffs -= Main_DrawInterface_Resources_Buffs_Hook;
        }

        private void Main_DrawInterface_Resources_Buffs_Hook(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Local variable indices
            const int num_idx = 0;
            const int num2_idx = 1;
            const int num5_idx = 2;
            const int i_idx = 3;
            const int x_idx = 4;
            const int num3_idx = 5;
            const int num4_idx = 6;

            // Add two new local variables
            int origI_idx = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Import(typeof(int))));

            int specialBuffSlot_idx = il.Body.Variables.Count;
            il.Body.Variables.Add(new VariableDefinition(il.Import(typeof(int))));

            // Init specialBuffSlot = -1
            c.Index = 0;
            c.Emit(OpCodes.Ldc_I4_M1);
            c.Emit(OpCodes.Stloc, specialBuffSlot_idx);

            // Find sequence after x = 32 + i * 38 line
            c.Index = 0;
            if (!c.TryGotoNext(
                instr => instr.MatchStloc(x_idx)))
            {
                throw new Exception("Failed to find x assignment pattern!");
            }

            // Move back to the start of the loop (before x = 32 + i * 38)
            c.Index -= 6;

            // origI = i
            c.Emit(OpCodes.Ldloc, i_idx);
            c.Emit(OpCodes.Stloc, origI_idx);

            // Modify i with advanced logic
            c.Emit(OpCodes.Ldloc, i_idx);
            c.Emit(OpCodes.Ldloc, specialBuffSlot_idx);
            c.Emit(OpCodes.Ldloc, num2_idx);
            c.EmitDelegate<Func<int, int, int, int>>((currentI, specialSlot, num2) =>
            {
                if (specialSlot == -1)
                    return currentI; // when current buff is before the Big Buff, or when Big Buff doesn't exist

                bool isZero = (specialSlot + 1) % num2 == 0;
                bool isGreaterOrEq = currentI >= specialSlot + (num2 - 1);
                bool isGreater = currentI > specialSlot + (num2 - 1);

                if (!isZero && !isGreaterOrEq) return currentI + 1; // when current buff is after Big buff but before clash with Big Buff on the next row AND Big buff is not on 11th slot (so it consumes 3 additional slots)
                if (isZero && isGreater) return currentI + 1; // when current buff is after the clash with Big Buff on the next row BUT Big buff is on 11th slot (so it consumes only 1 additional slot instead of 3)
                if (!isZero && isGreaterOrEq) return currentI + 3; // when current buff is after the clash with Big Buff on the next row AND Big buff is not on 11th slot (so it consumes 3 additional slots)

                return currentI;
            });
            c.Emit(OpCodes.Stloc, i_idx);

            // Find DrawBuffIcon call
            c.Index = 0;
            if (!c.TryGotoNext(
                instr => instr.MatchLdloc(num_idx),
                instr => instr.MatchLdloc(i_idx),
                instr => instr.MatchLdloc(x_idx),
                instr => instr.MatchLdloc(num3_idx),
                instr => instr.MatchCall<Main>("DrawBuffIcon")))
            {
                throw new Exception("Failed to find DrawBuffIcon call!");
            }

            // Position AFTER stloc.0 (DrawBuffIcon result)
            c.Index++;

            // Restore original i
            c.Emit(OpCodes.Ldloc, origI_idx);
            c.Emit(OpCodes.Stloc, i_idx);

            // Now we check for Big buff and update specialBuffSlot
            c.Emit(OpCodes.Ldloc, origI_idx);
            c.EmitDelegate<Func<int, bool>>((buffIdx) =>
            {
                Player player = Main.LocalPlayer;
                return player.buffTime[buffIdx] > 2 && player.buffType[buffIdx] == ModContent.BuffType<TrueSunflowerBuff>(); // Big buff timer is not 0 and buff type is Big buff
            });

            // If the buff is not valid, jump to label_buff_invalid
            var label_buff_invalid = c.DefineLabel();
            c.Emit(OpCodes.Brfalse_S, label_buff_invalid);

            // If this is a special buff - update specialBuffSlot = origI
            c.Emit(OpCodes.Ldloc, origI_idx);
            c.Emit(OpCodes.Stloc, specialBuffSlot_idx);

            c.MarkLabel(label_buff_invalid);
        }
    }
}
