using System.IO;

namespace Sunflowerology.Common.PacketHandlers
{
    internal class ModNetHandler
    {
        // Here we define the packet types we will be using
        public const byte PlantTE = 1;
        internal static PlantTEPackerHandler plantTEHandler = new(PlantTE);

        public static void HandlePacket(BinaryReader r, int fromWho)
        {
            // Here we read the packet type and call the appropriate handler
            switch (r.ReadByte())
            {
                case PlantTE:
                    plantTEHandler.HandlePacket(r, fromWho);
                    break;
                default:
                    Log.Warn("Unknown packet type: " + r.ReadByte());
                    break;
            }
        }
    }
}