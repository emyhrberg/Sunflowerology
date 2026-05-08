using Mono.Cecil.Cil;
using MonoMod.Cil;
using Sunflowerology.Common.PacketHandlers;
using Sunflowerology.Content.Buffs;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology
{
    public class Sunflowerology : Mod
    {
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModNetHandler.HandlePacket(reader, whoAmI);
        }
    }
}
