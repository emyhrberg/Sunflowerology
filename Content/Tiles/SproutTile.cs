using System;
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
        None = 0,
        OnRight = 1,
        OnLeft = -1
    }
    public class SproutEntity : ModTileEntity
    {
        public PairingState Pairing
        {
            get
            {
                if (CheckIfWalidPairing())
                {
                    int diff = pairedEntity.Position.X - Position.X;
                    return (PairingState)diff;
                }
                return PairingState.None;

            }
        }
        public SproutEntity pairedEntity;
        public bool IsPaired => Pairing != PairingState.None;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SproutTile>();
        }

        private bool CheckIfWalidPairing()
        {
            if (pairedEntity == null)
            {
                RemovePairing();
                return false;
            }
            int diff = Math.Abs(pairedEntity.Position.X - Position.X);
            if (diff != 1 || pairedEntity.Position.Y != Position.Y)
            {
                RemovePairing();
                return false;
            }
            return true;
        }

        public override void Update()
        {
            if (!IsPaired)
            {
                if (CheckUnpairedSprout(PairingState.OnRight)) return;
                if (CheckUnpairedSprout(PairingState.OnLeft)) return;
            }
            else
            {
                CheckIfWalidPairing();
                /*
                int pairedX = Position.X + (pairing == PairingState.OnRight ? 1 : -1);
                int pairedY = Position.Y;
                Tile pairedTile = Framing.GetTileSafely(pairedX, pairedY);

                if (!pairedTile.HasTile || pairedTile.IsActuated || pairedTile.TileType != ModContent.TileType<SproutTile>() ||
                    !TryGet(pairedX, pairedY, out SproutEntity other) ||
                    !other.IsPaired || other.pairing != GetOpposite(pairing))
                {
                    RemovePairing();
                }*/
            }
        }

        private bool CheckUnpairedSprout(PairingState side)
        {
            int i = Position.X + (int)side;
            int j = Position.Y;

            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.HasTile && !tile.IsActuated && tile.TileType == ModContent.TileType<SproutTile>() &&
                TryGet(i, j, out SproutEntity other) && !other.IsPaired)
            {
                other.pairedEntity = this;
                pairedEntity = other;

                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, i, j);
                return true;
            }

            return false;
        }

        public void SetUpPairedEntity(PairingState pairing)
        {
            if (pairing == PairingState.None)
            {
                RemovePairing();
                return;
            }
            int i = Position.X + (int)pairing;
            int j = Position.Y;

            Tile tile = Framing.GetTileSafely(i, j);

            if (tile.HasTile && !tile.IsActuated &&
                tile.TileType == ModContent.TileType<SproutTile>() && TryGet(i, j, out SproutEntity other))
            {
                pairedEntity = other;
                other.pairedEntity = this;

                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, i, j);
            }
        }

        public void RemovePairing()
        {
            pairedEntity = null;
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(Pairing)] = (int)Pairing;
        }

        public override void LoadData(TagCompound tag)
        {
            SetUpPairedEntity((PairingState)tag.GetInt(nameof(Pairing)));
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)Pairing);
        }

        public override void NetReceive(BinaryReader reader)
        {
            SetUpPairedEntity((PairingState)reader.ReadInt32());
        }

        public override void OnKill()
        {
            pairedEntity?.RemovePairing();
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
        }

        public static PairingState GetOpposite(PairingState state)
        {
            return state switch
            {
                PairingState.OnLeft => PairingState.OnRight,
                PairingState.OnRight => PairingState.OnLeft,
                _ => PairingState.None
            };
        }
    }

}
