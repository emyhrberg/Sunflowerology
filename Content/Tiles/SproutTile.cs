using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles
{
    internal class SproutTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Grass;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [18];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<SproutEntity>().Generic_HookPostPlaceMyPlayer;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            if (TileEntity.TryGet(i, j, out SproutEntity tileEntity))
            {
                if (tileEntity.Pairing == PairingState.OnRight)
                    tileFrameY = 20;
                else if (tileEntity.Pairing == PairingState.OnLeft)
                    tileFrameY = 40;
                else
                    tileFrameY = 0;
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            ModContent.GetInstance<SproutEntity>().Kill(i, j);
        }
    }
    public enum PairingState
    {
        None,
        OnRight,
        OnLeft
    }
    public class SproutEntity : ModTileEntity
    {

        public PairingState Pairing;

        public bool IsPaired => Pairing != PairingState.None;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SproutTile>();
        }
        public override void Update()
        {
            if (!IsPaired)
            {
                if (CheckUnpairedSprouts(Position.X, Position.Y, PairingState.OnRight))
                {
                    return;
                }
                if (CheckUnpairedSprouts(Position.X, Position.Y, PairingState.OnLeft))
                {
                    return;
                }
            }
            else
            {
                int pairedX = Position.X + (Pairing == PairingState.OnRight ? 1 : -1);
                int pairedY = Position.Y;
                Tile pairedTile = Framing.GetTileSafely(pairedX, pairedY);

                if (!pairedTile.HasTile || pairedTile.IsActuated || pairedTile.TileType != ModContent.TileType<SproutTile>() || !TryGet(pairedX, pairedY, out SproutEntity pairedEntity) || !pairedEntity.IsPaired || pairedEntity.Pairing != (Pairing == PairingState.OnRight ? PairingState.OnLeft : PairingState.OnRight))
                {
                    Pairing = PairingState.None;
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(Pairing)] = (int)Pairing;
        }

        public override void LoadData(TagCompound tag)
        {
            Pairing = (PairingState)tag.GetInt(nameof(Pairing));
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)Pairing);
        }

        public override void NetReceive(BinaryReader reader)
        {
            Pairing = (PairingState)reader.ReadInt32();
        }


        /// <summary>
        /// Checks for unpaired sprouts adjacent to the current sprout and pairs them if found.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="side"></param>
        /// <returns>Returns true if a pairing was made, false otherwise.</returns></returns>
        private bool CheckUnpairedSprouts(int x, int y, PairingState side)
        {
            int i = x + (side == PairingState.OnRight ? 1 : -1);
            int j = y;
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.HasTile && !tile.IsActuated &&
                tile.type == ModContent.TileType<SproutTile>() && TryGet(i, j, out SproutEntity tileEntityOnRight))
            {
                if (!tileEntityOnRight.IsPaired)
                {
                    tileEntityOnRight.Pairing = side == PairingState.OnRight ? PairingState.OnLeft : PairingState.OnRight;
                    Pairing = side;
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, x, y);
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, i, j);
                    return true; // Successfully paired
                }
            }
            return false;
        }
    }
}
