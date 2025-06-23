using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
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
            TileObjectData.newTile.AnchorValidTiles = SeedData.BlocksToSeedsProperties.Keys.ToArray();
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<SproutEntity>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 200, 0));
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

        public override void MouseOver(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out SproutEntity tileEntity))
            {
                Player player = Main.LocalPlayer;
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = -1;
                player.cursorItemIconText = $"Growth: {tileEntity.growthAmount}, Diff: {tileEntity.seedSurroundingDifference}";
                foreach (var seedTag in SeedTags.AllTags)
                {
                    player.cursorItemIconText += $"\n{seedTag}: {tileEntity.seedData[seedTag]}, S:" +
                        $"{tileEntity.surroundingAreaData[seedTag]}, D: " +
                        $"{tileEntity.seedSurroundingDifferenceDetailed[seedTag]}";
                }
            }
        }
    }

    public class SproutEntity : ModTileEntity
    {
        //Seed Data
        public SeedData seedData = new SeedData();
        public SeedData surroundingAreaData = new SeedData();
        public static SeedData transferData = new SeedData();
        public SeedData seedSurroundingDifferenceDetailed = new();
        public float seedSurroundingDifference = 0f;

        //Sprout Data
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
        public int growthAmount
        {
            get;
            set
            {
                field = Math.Clamp(value, 0, 100);
            }
        }
        public const int HowOftenUpdate = 30;
        public int updateCounter = 0;
        public static List<(int, int, SeedData, SeedData)> seedlingPosToGrow = new();


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
        /*
        private void GrowSeedling(int i, int j, SproutData sproutData1, SproutData sproutData2)
        {
            Random r = new Random();
            int rInt = r.Next(0, 3);
            if (WorldGen.PlaceObject(i, j, ModContent.TileType<SeedlingTile>(), mute: true, random: rInt))
            {
                int Type = ModContent.TileType<SeedlingTile>();
                TileObjectData tileData = TileObjectData.GetTileData(Type, 0);
                Point16 point = TileObjectData.TopLeft(i, j);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    NetMessage.SendTileSquare(Main.myPlayer, point.X, point.Y, tileData.Width, tileData.Height);
                    NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, point.X, point.Y, Type);
                }
                else
                {
                    int id = Place(point.X, point.Y);
                    if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is SeedlingEntity seedlingEntity)
                    {
                        seedlingEntity.sproutData1 = sproutData1;
                        seedlingEntity.sproutData2 = sproutData2;
                    }
                    NetMessage.SendObjectPlacement(-1, i, j, ModContent.TileType<SeedlingTile>(), 0, 0, -1, -1);
                }
                

            }
        }*/

        private static void GrowSeedling(int i, int j, SeedData sd, SeedData errorSd)
        {
            Random r = new Random();
            int rInt = r.Next(0, 3);
            if (!WorldGen.PlaceObject(i, j, ModContent.TileType<SeedlingTile>(), mute: true, random: rInt))
            {
                return;
            }

            int id = ModContent.GetInstance<SeedlingEntity>().Place(i, j - 1);
            if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is SeedlingEntity seedlingEntity)
            {
                RandomizeRedusingError(sd, errorSd, r, seedlingEntity);
            }
            NetMessage.SendObjectPlacement(-1, i, j, ModContent.TileType<SeedlingTile>(), 0, 0, -1, -1);
        }

        private static void RandomizeRedusingError(SeedData sd, SeedData errorSd, Random r, SeedlingEntity seedlingEntity)
        {
            var originalSproutData = sd;
            foreach (string seedTag in SeedTags.AllTags)
            {
                int rrInt = r.Next(0, 10);
                if (rrInt == 0)
                {
                    seedlingEntity.seedData[seedTag] =
                        (int)Math.Round(originalSproutData[seedTag] - ((float)errorSd[seedTag] * 2f / 3f));
                }
                else if (rrInt > 0 && rrInt < 9)
                {
                    seedlingEntity.seedData[seedTag] =
                        (int)Math.Round(originalSproutData[seedTag] + (float)errorSd[seedTag] / 4f);
                }
                else if (rrInt == 9)
                {
                    seedlingEntity.seedData[seedTag] =
                        (int)Math.Round(originalSproutData[seedTag] + (float)errorSd[seedTag] * 3f / 4f);
                }
            }
        }

        private void CheckIfGrowenUp()
        {
            if (pairedEntity?.growthAmount >= 100 && growthAmount >= 100 && Pairing == PairingState.OnRight)
            {
                var sd = (seedData + pairedEntity.seedData) / 2;

                int i = Position.X;
                int j = Position.Y;

                // Kill the current tile and the paired entity's tile
                WorldGen.KillTile(i, j, false, false, true);
                WorldGen.KillTile(pairedEntity.Position.X, pairedEntity.Position.Y, false, false, true);

                // Place the new tile

                seedlingPosToGrow.Add((i, j, sd, seedSurroundingDifferenceDetailed));
            }
        }
        private void CalculateSurroundings()
        {
            surroundingAreaData = new SeedData();
            Tile[] Tiles25p = [Framing.GetTileSafely(Position.X, Position.Y + 1),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + 1)];
            Tile[] Tiles125p = [Framing.GetTileSafely(Position.X, Position.Y + 2),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + 2)];
            Tile[] Tiles625p = [Framing.GetTileSafely(Position.X - 1, Position.Y + 1),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + 1),
                                Framing.GetTileSafely(Position.X - 1, Position.Y + 2),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + 2),];
            foreach (Tile tile in Tiles25p)
            {
                if (tile.HasTile && SeedData.BlocksToSeedsProperties.TryGetValue(tile.TileType, out SeedData seedData))
                {
                    surroundingAreaData += seedData * 0.25f;
                }
            }
            foreach (Tile tile in Tiles125p)
            {
                if (tile.HasTile && SeedData.BlocksToSeedsProperties.TryGetValue(tile.TileType, out SeedData seedData))
                {
                    surroundingAreaData += seedData * 0.125f;
                }
            }
            foreach (Tile tile in Tiles625p)
            {
                if (tile.HasTile && SeedData.BlocksToSeedsProperties.TryGetValue(tile.TileType, out SeedData seedData))
                {
                    surroundingAreaData += seedData * 0.0625f;
                }
            }
            if (SeedData.DepthZoneToSeedAffinity.TryGetValue(Depth.GetDepthZone(Position.Y), out var i))
            {
                surroundingAreaData += i * 0.25f;
            }
            surroundingAreaData = surroundingAreaData.Normalize();
        }
        private void CalculateDifference()
        {
            seedSurroundingDifferenceDetailed = surroundingAreaData - (seedData + pairedEntity.seedData) / 2;
            var diff = seedSurroundingDifferenceDetailed.Clone();
            foreach (var seedTag in SeedTags.AllTags)
            {
                diff[seedTag] = Math.Abs(diff[seedTag]);
            }
            seedSurroundingDifference = diff.AverageLove;
        }
        public override void PostGlobalUpdate()
        {
            foreach (var (i, j, sd, errorSd) in seedlingPosToGrow)
            {
                GrowSeedling(i, j, sd, errorSd);
            }
            seedlingPosToGrow.Clear();
        }
        public override void Update()
        {
            // Update only every HowOftenUpdate frames
            updateCounter++;
            updateCounter %= HowOftenUpdate;
            if (updateCounter != 0)
            {
                return;
            }


            if (!IsPaired)
            {
                if (CheckUnpairedSprout(PairingState.OnRight)) return;
                if (CheckUnpairedSprout(PairingState.OnLeft)) return;
            }
            else
            {
                if (Pairing == PairingState.OnRight)
                {
                    CalculateSurroundings();
                    pairedEntity.surroundingAreaData = surroundingAreaData;
                    CalculateDifference();

                }
                else if (Pairing == PairingState.OnLeft)
                {
                    CalculateDifference();
                }

                if (Main.rand.NextBool((int)seedSurroundingDifference + 1))
                {
                    growthAmount += 10;
                }
                CheckIfGrowenUp();
            }
        }
        public override void SaveData(TagCompound tag)
        {
            tag[nameof(Pairing)] = (int)Pairing;
            tag[nameof(growthAmount)] = growthAmount;
            foreach (var seedTag in SeedTags.AllTags)
            {
                tag[seedTag] = seedData[seedTag];
            }
        }
        public override void LoadData(TagCompound tag)
        {
            SetUpPairedEntity((PairingState)tag.GetInt(nameof(Pairing)));
            growthAmount = tag.GetInt(nameof(growthAmount));
            foreach (var seedTag in SeedTags.AllTags)
            {
                try
                {
                    if (tag.TryGet(seedTag, out int val))
                    {
                        seedData[seedTag] = val;
                    }
                    else
                    {
                        seedData[seedTag] = 0;
                    }
                }
                catch
                {
                    seedData[seedTag] = 0; // If the tag is not found or fails, set it to 0
                }

            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((int)Pairing);
            writer.Write(growthAmount);
            foreach (var seedTag in SeedTags.AllTags)
            {
                writer.Write(seedData[seedTag]);
            }

        }
        public override void NetReceive(BinaryReader reader)
        {
            SetUpPairedEntity((PairingState)reader.ReadInt32());
            growthAmount = reader.ReadInt32();
            foreach (var seedTag in SeedTags.AllTags)
            {
                seedData[seedTag] = reader.ReadInt32();
            }
        }
        public override void OnKill()
        {
            pairedEntity?.RemovePairing();
            NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, Type, Position.X, Position.Y);
        }
        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            TileObjectData tileData = TileObjectData.GetTileData(type, style, alternate);
            Point16 point = TileObjectData.TopLeft(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, point.X, point.Y, tileData.Width, tileData.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, point.X, point.Y, Type);
                if (TileEntity.TryGet(point.X, point.Y, out SproutEntity entity))
                {
                    entity.seedData = transferData;
                }

                return -1;
            }
            int result = Place(point.X, point.Y);
            if (TileEntity.TryGet(point.X, point.Y, out SproutEntity tileEntity))
            {
                tileEntity.seedData = transferData;
            }
            return result;
        }

    }
    public enum PairingState
    {
        None = 0,
        OnRight = 1,
        OnLeft = -1
    }
    public enum DepthZone
    {
        Sky,
        Overworld,
        DirtLayer,
        RockLayer,
        Underworld
    }
    public class SeedData
    {
        public static readonly Dictionary<int, SeedData> BlocksToSeedsProperties = new()
        {
            // Overworld blocks and their variations
            [TileID.Grass] = new SeedData(
        (SeedTags.Sun, 100), (SeedTags.Water, 40), (SeedTags.Wild, 30), (SeedTags.Dry, 10)),
            [TileID.Dirt] = new SeedData(
        (SeedTags.Sun, 70), (SeedTags.Dry, 50), (SeedTags.Water, 20), (SeedTags.Cave, 10)),

            // Stone and moss blocks
            [TileID.Stone] = new SeedData(
        (SeedTags.Cave, 100), (SeedTags.Dry, 60), (SeedTags.Cold, 30)),
            [TileID.GreenMoss] = new SeedData(
        (SeedTags.Cave, 75), (SeedTags.Water, 65), (SeedTags.Wild, 60)),
            [TileID.BlueMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Cold, 40)),
            [TileID.ArgonMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Good, 40)),
            [TileID.BrownMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Dry, 20)),
            [TileID.KryptonMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 70), (SeedTags.Wild, 90)),
            [TileID.PurpleMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Evil, 50)),
            [TileID.RainbowMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Evil, 20), (SeedTags.Good, 20), (SeedTags.Honey, 50)),
            [TileID.RedMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Evil, 50)),
            [TileID.VioletMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Water, 30)),
            [TileID.XenonMoss] = new SeedData(
        (SeedTags.Cave, 70), (SeedTags.Water, 80), (SeedTags.Cold, 30)),
            [TileID.LavaMoss] = new SeedData(
        (SeedTags.Cave, 85), (SeedTags.Hot, 70), (SeedTags.Dry, 50)),

            // Dry and sandy blocks
            [TileID.Sand] = new SeedData(
        (SeedTags.Dry, 100), (SeedTags.Hot, 80), (SeedTags.Sun, 60), (SeedTags.Water, 10)),
            [TileID.HardenedSand] = new SeedData(
        (SeedTags.Dry, 90), (SeedTags.Hot, 70), (SeedTags.Cave, 40)),
            [TileID.Sandstone] = new SeedData(
        (SeedTags.Dry, 80), (SeedTags.Cave, 60), (SeedTags.Hot, 50)),

            // Snow and ice blocks
            [TileID.SnowBlock] = new SeedData(
        (SeedTags.Cold, 100), (SeedTags.Sun, 60), (SeedTags.Water, 40)),
            [TileID.IceBlock] = new SeedData(
        (SeedTags.Cold, 100), (SeedTags.Cave, 50), (SeedTags.Water, 80)),

            // Jungle blocks and mud
            [TileID.JungleGrass] = new SeedData(
        (SeedTags.Wild, 100), (SeedTags.Water, 100), (SeedTags.Sun, 50), (SeedTags.Hot, 30)),
            [TileID.Mud] = new SeedData(
        (SeedTags.Wild, 80), (SeedTags.Water, 100), (SeedTags.Sun, 20)),

            // Hallowed blocks and their variations
            [TileID.HallowedGrass] = new SeedData(
        (SeedTags.Good, 100), (SeedTags.Sun, 80), (SeedTags.Water, 30), (SeedTags.Wild, 20)),
            [TileID.Pearlstone] = new SeedData(
        (SeedTags.Good, 90), (SeedTags.Cave, 70), (SeedTags.Water, 20)),
            [TileID.Pearlsand] = new SeedData(
        (SeedTags.Good, 80), (SeedTags.Sun, 60), (SeedTags.Dry, 50)),
            [TileID.HallowedIce] = new SeedData(
        (SeedTags.Good, 100), (SeedTags.Cold, 80), (SeedTags.Water, 50)),

            // Evil blocks and their variations
            [TileID.CorruptGrass] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Sun, 30), (SeedTags.Dry, 40), (SeedTags.Water, 10)),
            [TileID.Ebonstone] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Cave, 60), (SeedTags.Dry, 40)),
            [TileID.Crimstone] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Cave, 60), (SeedTags.Dry, 40)),
            [TileID.CrimsonGrass] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Sun, 30), (SeedTags.Water, 30), (SeedTags.Hot, 20)),
            [TileID.Crimsand] = new SeedData(
        (SeedTags.Evil, 90), (SeedTags.Sun, 50), (SeedTags.Dry, 60), (SeedTags.Hot, 20)),
            [TileID.Ebonsand] = new SeedData(
        (SeedTags.Evil, 90), (SeedTags.Sun, 50), (SeedTags.Dry, 60), (SeedTags.Hot, 20)),
            [TileID.FleshIce] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Cold, 80), (SeedTags.Water, 50), (SeedTags.Dry, 20)),
            [TileID.CorruptIce] = new SeedData(
        (SeedTags.Evil, 100), (SeedTags.Cold, 80), (SeedTags.Water, 50), (SeedTags.Dry, 20)),

            // Hell blocks and their variations
            [TileID.Ash] = new SeedData(
        (SeedTags.Hot, 100), (SeedTags.Dry, 100), (SeedTags.Cave, 80), (SeedTags.Evil, 20)),
            [TileID.Hellstone] = new SeedData(
        (SeedTags.Hot, 100), (SeedTags.Cave, 90), (SeedTags.Dry, 80), (SeedTags.Evil, 50)),
            [TileID.AshGrass] = new SeedData(
        (SeedTags.Hot, 60), (SeedTags.Dry, 50), (SeedTags.Cave, 90), (SeedTags.Evil, 10)),
            [TileID.Obsidian] = new SeedData(
                (SeedTags.Hot, 100), (SeedTags.Cave, 80), (SeedTags.Cold, 100)),

            // Honey blocks and their variations
            [TileID.HoneyBlock] = new SeedData(
        (SeedTags.Honey, 100), (SeedTags.Water, 80), (SeedTags.Sun, 40), (SeedTags.Wild, 30)),
            [TileID.CrispyHoneyBlock] = new SeedData(
        (SeedTags.Honey, 90), (SeedTags.Dry, 70), (SeedTags.Sun, 50), (SeedTags.Hot, 40)),
            [TileID.BeeHive] = new SeedData(
        (SeedTags.Honey, 100), (SeedTags.Sun, 90), (SeedTags.Water, 60)),

            // Clouds and their variations
            [TileID.Cloud] = new SeedData(
        (SeedTags.Sun, 100), (SeedTags.Water, 90), (SeedTags.Cold, 30)),
            [TileID.RainCloud] = new SeedData(
        (SeedTags.Sun, 100), (SeedTags.Water, 100), (SeedTags.Cold, 40)),
            [TileID.SnowCloud] = new SeedData(
        (SeedTags.Sun, 100), (SeedTags.Cold, 100), (SeedTags.Water, 70)),

            // Ocean blocks and their variations
            [TileID.Coralstone] = new SeedData(
        (SeedTags.Water, 100), (SeedTags.Sun, 60), (SeedTags.Wild, 50)),
            [TileID.ShellPile] = new SeedData(
        (SeedTags.Water, 90), (SeedTags.Sun, 30), (SeedTags.Dry, 20)),
        };
        public static readonly Dictionary<DepthZone, SeedData> DepthZoneToSeedAffinity = new()
        {
            [DepthZone.Sky] = new SeedData((SeedTags.Sun, 100), (SeedTags.Cave, -100), (SeedTags.Cold, 20)),
            [DepthZone.Overworld] = new SeedData((SeedTags.Sun, 50), (SeedTags.Cave, -50)),
            [DepthZone.DirtLayer] = new SeedData((SeedTags.Cave, 20), (SeedTags.Sun, 30)),
            [DepthZone.RockLayer] = new SeedData((SeedTags.Cave, 80), (SeedTags.Hot, 10), (SeedTags.Sun, -50)),
            [DepthZone.Underworld] = new SeedData((SeedTags.Hot, 100), (SeedTags.Cave, 60), (SeedTags.Sun, -100)),
        };

        private readonly Dictionary<string, int> loves = new();

        public SeedData() { }

        public SeedData(params (string key, int value)[] entries)
        {
            foreach (var (key, value) in entries)
                loves[key] = value;
        }

        public int this[string key]
        {
            get => loves.TryGetValue(key, out var v) ? v : 0;
            set => loves[key] = value;
        }

        public IEnumerable<string> Keys => loves.Keys;
        public IEnumerable<int> Values => loves.Values;
        public IReadOnlyDictionary<string, int> ValuesMap => loves;

        public static SeedData operator +(SeedData a, SeedData b)
        {
            var result = new SeedData();
            foreach (var key in AllKeys(a, b))
                result[key] = a[key] + b[key];
            return result;
        }

        public static SeedData operator -(SeedData a, SeedData b)
        {
            var result = new SeedData();
            foreach (var key in AllKeys(a, b))
                result[key] = a[key] - b[key];
            return result;
        }

        public static SeedData operator *(SeedData a, float multiplier)
        {
            var result = new SeedData();
            foreach (var kvp in a.loves)
                result[kvp.Key] = (int)Math.Round(kvp.Value * multiplier);
            return result;
        }

        public static SeedData operator /(SeedData a, float divisor)
        {
            var result = new SeedData();
            foreach (var kvp in a.loves)
                result[kvp.Key] = (int)Math.Round(kvp.Value / divisor);
            return result;
        }

        public float TotalLove => loves.Values.Sum();

        public float AverageLove => TotalLove / SeedTags.Count;

        private static HashSet<string> AllKeys(SeedData a, SeedData b)
        {
            var keys = new HashSet<string>(a.loves.Keys);
            keys.UnionWith(b.loves.Keys);
            return keys;
        }

        public override string ToString()
        {
            return string.Join(", ", loves.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        public SeedData Normalize()
        {
            SeedData normalized = new SeedData();
            foreach (var key in loves.Keys.ToList())
            {
                normalized[key] = Math.Clamp(loves[key], 0, 100); // Ensure values are between 0 and 100
            }
            return normalized;
        }

        public SeedData Clone()
        {
            SeedData clone = new SeedData();
            foreach (var kvp in loves)
            {
                clone[kvp.Key] = kvp.Value;
            }
            return clone;
        }
    }
    public static class SeedTags
    {
        public const string Dry = "DryLove";
        public const string Water = "WaterLove";
        public const string Wild = "WildLove";
        public const string Sun = "SunLove";
        public const string Cave = "CaveLove";
        public const string Hot = "HotLove";
        public const string Cold = "ColdLove";
        public const string Evil = "EvilLove";
        public const string Good = "GoodLove";
        public const string Honey = "HoneyLove";
        public static string[] AllTags =
        [
            Dry, Water, Wild, Sun, Cave, Hot, Cold, Evil, Good, Honey
        ];
        public static int Count => AllTags.Count(); // Total number of tags, used for validation
    }

    public static class Depth
    {
        public static DepthZone GetDepthZone(int tileY)
        {
            if (tileY > Main.UnderworldLayer)
                return DepthZone.Underworld;
            if (tileY <= Main.UnderworldLayer && tileY > Main.rockLayer)
                return DepthZone.RockLayer;
            if (tileY <= Main.rockLayer && tileY > Main.worldSurface)
                return DepthZone.DirtLayer;
            if (tileY <= Main.worldSurface && tileY > Main.worldSurface * 0.35)
                return DepthZone.Overworld;
            return DepthZone.Sky;
        }


    }

}

