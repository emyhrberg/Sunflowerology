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

        /// <summary>
        /// Checks if the pairing with the paired entity is valid.
        /// </summary>
        /// <returns></returns>
        private bool CheckIfWalidPairing()
        {
            // Check if the paired entity is not null
            if (pairedEntity == null)
            {
                return false;
            }

            // Calculate the difference in X position
            int diff = Math.Abs(pairedEntity.Position.X - Position.X);

            // Check if the paired entity is on the same Y position and is exactly one tile away in X direction
            if (diff != 1 || pairedEntity.Position.Y != Position.Y)
            {
                RemovePairing();
                return false;
            }

            // If all checks pass, the pairing is valid
            return true;
        }

        /// <summary>
        /// Checks if there is an unpaired sprout on the specified side and pairs it with this entity if found.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private bool CheckUnpairedSprout(PairingState side)
        {
            // Set the coordinates based on the side
            int i = Position.X + (int)side;
            int j = Position.Y;

            // Get the tile at the specified coordinates
            Tile tile = Framing.GetTileSafely(i, j);

            // Check if the tile is valid for pairing
            if (tile.HasTile && !tile.IsActuated && tile.TileType == ModContent.TileType<SproutTile>() &&
                TryGet(i, j, out SproutEntity other) && !other.IsPaired)
            {
                // Pair the entities
                other.pairedEntity = this;
                pairedEntity = other;

                // Send the pairing information to the network
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, i, j);
                return true;
            }

            // If no unpaired sprout is found on the specified side, return false
            return false;
        }

        /// <summary>
        /// Sets up the paired entity based on the pairing state.
        /// </summary>
        /// <param name="pairing"></param>
        public void SetUpPairedEntity(PairingState pairing)
        {
            // If the pairing state is None, remove the pairing.
            if (pairing == PairingState.None)
            {
                RemovePairing();
                return;
            }

            // Positio of the potential paired entity
            int i = Position.X + (int)pairing;
            int j = Position.Y;

            // Get the tile at the specified coordinates
            Tile tile = Framing.GetTileSafely(i, j);

            // If the tile is valid for pairing.
            if (tile.HasTile && !tile.IsActuated &&
                tile.TileType == ModContent.TileType<SproutTile>() && TryGet(i, j, out SproutEntity other))
            {
                // Pair the entities
                pairedEntity = other;
                other.pairedEntity = this;

                // Send the pairing information to the network
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, i, j);
            }
        }

        /// <summary>
        /// Removes the pairing of this entity with its paired entity, if any.
        /// </summary>
        public void RemovePairing()
        {
            pairedEntity = null;
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
        }

        //TODO: Slow down the update rate of this entity, so it doesn't check every frame.
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
            }
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


    }

}
