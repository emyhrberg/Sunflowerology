using Sunflowerology.Content.Items.SunflowerSeeds;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Sunflowerology.Common.PacketHandlers
{
    internal class PlantTEPackerHandler : BasePacketHandler
    {
        public const byte PlacingTE = 1;
        public const byte KillingTE = 2;
        public PlantTEPackerHandler(byte handlerType) : base(handlerType) { }

        // Only for server:
        private byte majorClient = 0;
        private bool shouldServerBeSaved = false;

        //Gets packets
        public override void HandlePacket(BinaryReader reader, int fromWho)
        {
            switch (reader.ReadByte())
            {
                case PlacingTE:
                    ReceivePlacingTE(reader, fromWho);
                    break;
                case KillingTE:
                    ReceiveKillingTE(reader, fromWho);
                    break;
            }
        }

        //MP client:
        public void SendPlacingTE(int toWho, int ignoreWho, int i, int j, SeedItem seedItem)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = GetPacket(PlacingTE);
                packet.Write(i);
                packet.Write(j);
                foreach (var seedTag in NatureTags.AllTags)
                {
                    packet.Write(seedItem.seedData[seedTag]);
                }
                packet.Send(toWho, ignoreWho);
            }
        }

        //Server:
        public void ReceivePlacingTE(BinaryReader reader, int fromWho)
        {
            Log.Info($"Receiving PlacingTE to {Main.myPlayer} from {fromWho}");

            if (Main.netMode == NetmodeID.Server)
            {
                int i = reader.ReadInt32();
                int j = reader.ReadInt32();
                NatureData natureData = new NatureData();
                foreach (var seedTag in NatureTags.AllTags)
                {
                    natureData[seedTag] = reader.ReadInt32();
                }
                Log.Info($"Placing TE at {i}, {j} with data: {natureData}");
                int id = ModContent.GetInstance<SproutEntity>().Place(i, j);
                if (id != -1 && TileEntity.ByID.TryGetValue(id, out var te) && te is SproutEntity sproutTE)
                {
                    sproutTE.plantData = natureData;
                    NetMessage.SendTileSquare(fromWho, i, j, 1, TileChangeType.None);
                    NetMessage.SendData(MessageID.TileEntitySharing, number: id);
                }
                else
                {
                    Log.Warn($"Failed to place SproutEntity at {i}, {j}");
                }

            }
        }

        //MP client:
        public void SendKillingTE(int toWho, int ignoreWho, int id)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = GetPacket(KillingTE);
                packet.Write(id);
                packet.Send(toWho, ignoreWho);
            }
        }

        //Server:
        public void ReceiveKillingTE(BinaryReader reader, int fromWho)
        {
            Log.Info($"Receiving KillingTE from {fromWho}");
            if (Main.netMode == NetmodeID.Server)
            {
                int id = reader.ReadInt32();
                Log.Info($"Killing TE with ID: {id}");
                if (TileEntity.ByID.TryGetValue(id, out TileEntity te))
                {
                    ((ModTileEntity)te).OnKill();
                    TileEntity.ByID.Remove(te.ID);
                    TileEntity.ByPosition.Remove(te.Position);
                    NetMessage.SendData(MessageID.TileEntitySharing, number: id);
                }
                else
                {
                    Log.Warn($"TileEntity with ID {id} not found.");
                }
            }
        }
    }
}