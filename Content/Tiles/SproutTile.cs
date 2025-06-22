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
                if (tileEntity.sproutData.pairing == PairingState.OnRight)
                    tileFrameY = 20;
                else if (tileEntity.sproutData.pairing == PairingState.OnLeft)
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

        public SproutData sproutData;

        public SproutEntity() : base()
        {
            sproutData = new SproutData()
            {
                parent = this,
                pairing = PairingState.None,
                pairedSproudData = null
            };
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SproutTile>();
        }

        public override void Update()
        {
            if (!sproutData.IsPaired)
            {
                if (sproutData.CheckUnpairedSprouts(PairingState.OnRight))
                {
                    return;
                }
                if (sproutData.CheckUnpairedSprouts(PairingState.OnLeft))
                {
                    return;
                }
            }
            else
            {
                int pairedX = Position.X + (sproutData.pairing == PairingState.OnRight ? 1 : -1);
                int pairedY = Position.Y;
                Tile pairedTile = Framing.GetTileSafely(pairedX, pairedY);

                if (!pairedTile.HasTile || pairedTile.IsActuated || pairedTile.TileType != ModContent.TileType<SproutTile>() || !TryGet(pairedX, pairedY, out SproutEntity pairedEntity) || !pairedEntity.sproutData.IsPaired || pairedEntity.sproutData.pairing != SproutData.GetOposite(sproutData.pairing))
                {
                    sproutData.RemovePairing();
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(sproutData.pairing)] = (int)sproutData.pairing;
        }

        public override void LoadData(TagCompound tag)
        {
            sproutData = new SproutData()
            {
                parent = this,
                pairing = (PairingState)tag.GetInt(nameof(sproutData.pairing))
            };
            sproutData.SetUpPairedSproutData();
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)sproutData.pairing);
        }

        public override void NetReceive(BinaryReader reader)
        {
            sproutData.pairing = (PairingState)reader.ReadInt32();
        }

        override public void OnKill()
        {
            sproutData.pairedSproudData?.RemovePairing();
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
        }


    }

    public class SproutData
    {
        public ModTileEntity parent;
        public PairingState pairing;
        public int X => parent.Position.X;
        public int Y => parent.Position.Y;
        public int TypeOfEntity => parent.Type;
        public SproutData pairedSproudData;
        public bool IsPaired => pairing != PairingState.None;

        /// <summary>
        /// Checks for an unpaired sprout on the specified side (left or right) and pairs it with this sprout if found.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public bool CheckUnpairedSprouts(PairingState side)
        {
            // Coordinates for the paired sprout based on the current sprout's position and pairing side
            int i = X + (side == PairingState.OnRight ? 1 : -1);
            int j = Y;

            // Get the tile at the paired coordinates
            Tile tile = Framing.GetTileSafely(i, j);

            // Check if the tile is a sprout tile and if it has a tile entity
            if (tile.HasTile && !tile.IsActuated &&
                tile.type == ModContent.TileType<SproutTile>() && TileEntity.TryGet(i, j, out SproutEntity tileEntityOnSide))
            {
                // If the tile entity is found and it is not paired, pair it with this sprout
                if (!tileEntityOnSide.sproutData.IsPaired)
                {
                    tileEntityOnSide.sproutData.pairing = GetOposite(side);
                    pairing = side;
                    tileEntityOnSide.sproutData.pairedSproudData = this;
                    pairedSproudData = tileEntityOnSide.sproutData;
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, TypeOfEntity, X, Y);
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, TypeOfEntity, i, j);
                    return true; // Successfully paired
                }
            }
            return false;
        }

        public void SetUpPairedSproutData()
        {
            if (!IsPaired) { return; }
            if (!GetPairedSproutData())
            {
                //Log.Warn($"Sprout at ({x}, {y}) with pairing {pairing} could not find a paired sprout. This may cause issues in the future.");
            }
            else
            {
                //Log.Info($"Sprout at ({x}, {y}) successfully paired with sprout at ({pairedSproudData.x}, {pairedSproudData.y}) with pairing {pairedSproudData.pairing}.");
            }
        }

        /// <summary>
        /// Attempts to find and set the paired sprout data based on the current sprout's position and pairing state.
        /// </summary>
        /// <returns>True if a paired sprout was found and set, false otherwise.</returns>
        private bool GetPairedSproutData()
        {
            // Coordinates for the paired sprout based on the current sprout's position and pairing side
            int i = X + (pairing == PairingState.OnRight ? 1 : -1);
            int j = Y;

            // Get the tile at the paired coordinates
            Tile tile = Framing.GetTileSafely(i, j);

            // Check if the tile is a sprout tile and if it has a tile entity
            if (tile.HasTile && !tile.IsActuated &&
                tile.type == ModContent.TileType<SproutTile>() && TileEntity.TryGet(i, j, out SproutEntity tileEntityOnRight))
            {
                // If the tile entity is found and its pairing state matches the opposite of the current sprout's pairing state,
                if (tileEntityOnRight.sproutData.pairing == GetOposite(pairing))
                {
                    // Set the paired sprout data
                    tileEntityOnRight.sproutData.pairedSproudData = this;
                    pairedSproudData = tileEntityOnRight.sproutData;
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, TypeOfEntity, X, Y);
                    NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, TypeOfEntity, i, j);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the pairing of this sprout with its paired sprout, if any.
        /// </summary>
        internal void RemovePairing()
        {
            pairedSproudData = null;
            pairing = PairingState.None;
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, TypeOfEntity, X, Y);
        }

        /// <summary>
        /// Returns the opposite pairing state of the given state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static PairingState GetOposite(PairingState state)
        {
            if (state == PairingState.OnRight)
            {
                return PairingState.OnLeft;
            }
            else if (state == PairingState.OnLeft)
            {
                return PairingState.OnRight;
            }
            return PairingState.None;
        }
    }
}
