using Sunflowerology.Common.PacketHandlers;
using System.IO;
using Terraria.ModLoader;

namespace Sunflowerology
{
    public class Sunflowerology : Mod
    {
        // You probably shouldnt add code here
        // If you can add the code to another class, you should do that instead
        // This is the entire mod class and is meant for other things than grass seeds
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            ModNetHandler.HandlePacket(reader, whoAmI);
        }
    }
}
