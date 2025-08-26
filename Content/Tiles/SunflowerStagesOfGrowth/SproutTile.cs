using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sunflowerology.Common.PacketHandlers;
using Sunflowerology.Content.Items.Sunflowers;
using Sunflowerology.Content.Items.SunflowerSeeds;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SproutTile : PlantStageTile<SproutEntity>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            var tileData = TileObjectData.GetTileData(Type, 0);
            tileData.AnchorValidTiles = NatureData.TilesToData.Keys.ToArray();
            tileData.StyleWrapLimit = 3;
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            if (TileEntity.TryGet(i, j, out SproutEntity tileEntity))
            {
                if (tileEntity.Pairing == PairingState.OnRight)
                    tileFrameY += 20;
                else if (tileEntity.Pairing == PairingState.OnLeft)
                    tileFrameY += 40;
                else
                    tileFrameY += 0;
            }
        }
        protected override int WidthInTiles => 1;
        protected override int HeightInTiles => 1;

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            base.KillTile(i, j, ref fail, ref effectOnly, ref noItem);
            noItem = true;
        }

        protected override bool IsSpecialHook => true;

        protected override bool HaveGlow => false;

        protected override int[] Heights => [18];

        public override void PlaceInWorld(int i, int j, Item item)
        {
            TileObjectData tileData = TileObjectData.GetTileData(Type, 0);
            var topLeft = TileObjectData.TopLeft(i, j);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModNetHandler.plantTEHandler.SendPlacingTE(-1, -1, topLeft.X, topLeft.Y, (SeedItem)item.ModItem);
                return;
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                int id = ModContent.GetInstance<SproutEntity>().Place(i, j);
                if (id != -1 && TileEntity.ByID.TryGetValue(id, out var te) && te is SproutEntity sproutTE)
                {
                    sproutTE.plantData = ((SeedItem)item.ModItem).seedData.Clone();
                }
                else
                {
                    Log.Warn($"Failed to place SproutEntity at {i}, {j}");
                }
            }
        }
    }
    public class SproutEntity : PlantStageEntity<SeedlingEntity>
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

        protected override int TileType => ModContent.TileType<SproutTile>();

        protected override int NextTileType => ModContent.TileType<SeedlingTile>();

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

        public override void Update()
        {
            // If the type of sunflower is not set, find the closest type based on the plant data
            if (typeOfSunflower == TypeOfSunflower.None && !IsDead)
            {
                typeOfSunflower = plantData.FindClosestTypeOfSunflower();
            }

            // Skip the update (to not update every frame)
            if (SkipUpdate()) return;

            // Updating the entity based on the pairing state
            if (!IsPaired)
            {
                // Reset growth level if not paired
                growthLevel = 0;

                // Check for unpaired sprouts on both sides
                if (!CheckAndPairUnpairedSprout(PairingState.OnRight))
                {
                    CheckAndPairUnpairedSprout(PairingState.OnLeft);
                }
            }
            else if (Pairing == PairingState.OnRight)
            {
                ModContent.GetModTile(NextTileType);
                // Calculate the surroundings area data
                surroundingAreaData = CalculateSurroundings();
                pairedEntity.surroundingAreaData = surroundingAreaData;

                // Calculate the difference
                difference = surroundingAreaData - ((plantData + pairedEntity.plantData) / 2);
                pairedEntity.difference = difference;

                // Calculate the average difference
                averageDifference = CalculateAverageDifference();
                pairedEntity.averageDifference = averageDifference;

                // Calculate the growth level
                float growthAdd = CalculateGrowth();
                growthLevel += growthAdd;
                pairedEntity.growthLevel = growthLevel;

                // Check if both sprouts are fully grown
                if (IsFullyGrown && pairedEntity.IsFullyGrown)
                    ReplacePlantWithNewOne();
            }
            else if (Pairing == PairingState.OnLeft)
            {
            }

            // Send the updated data to the server
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
        }

        protected override void ReplacePlantWithNewOne()
        {
            var tileAboveLeft = Framing.GetTileSafely(Position.X, Position.Y - 1);
            var tileAboveRight = Framing.GetTileSafely(Position.X + 1, Position.Y - 1);
            if (tileAboveLeft.HasTile || tileAboveRight.HasTile)
            {
                return;
            }
            WorldGen.KillTile(Position.X, Position.Y, false, false, true);
            WorldGen.KillTile(pairedEntity.Position.X, pairedEntity.Position.Y, false, false, true);
            pairedEntity.Kill(pairedEntity.Position.X, pairedEntity.Position.Y);
            Kill(Position.X, Position.Y);

            growthQueue.Add((Position.X, Position.Y - 1, (plantData + pairedEntity.plantData) / 2, difference.Clone(), IsDead || pairedEntity.IsDead));
            NetMessage.SendTileSquare(-1, Position.X, Position.Y, 2, 2, TileChangeType.None);
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
            NetMessage.SendData(MessageID.TileEntitySharing, number: pairedEntity.ID);
        }

        protected override float CalculateGrowth()
        {
            var res = base.CalculateGrowth();
            if (IsPaired && IsDead)
            {
                if(!pairedEntity.IsDead)
                    pairedEntity.typeOfSunflower = TypeOfSunflower.Deadflower;
            }
            return res;
        }

        /// <summary>
        /// Checks if there is an unpaired sprout on the specified side and pairs it with this entity if found.
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        private bool CheckAndPairUnpairedSprout(PairingState side)
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
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
                NetMessage.SendData(MessageID.TileEntitySharing, number: pairedEntity.ID);
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
                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
                    NetMessage.SendData(MessageID.TileEntitySharing, number: pairedEntity.ID);
                }
            }
        }

        /// <summary>
        /// Removes the pairing of this entity with its paired entity, if any.
        /// </summary>
        public void RemovePairing()
        {
            pairedEntity = null;
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
        }
        public override void OnKill()
        {
            pairedEntity?.RemovePairing();
        }
        override public void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag[nameof(Pairing)] = (int)Pairing;
        }
        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.TryGet(nameof(Pairing), out int pairingState))
            {
                SetUpPairedEntity((PairingState)pairingState);
            }
            else
            {
                RemovePairing();
            }
        }
        override public void NetSend(BinaryWriter writer)
        {
            base.NetSend(writer);
            writer.Write((int)Pairing);
        }
        override public void NetReceive(BinaryReader reader)
        {
            base.NetReceive(reader);
            SetUpPairedEntity((PairingState)reader.ReadInt32());
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
    public class NatureData
    {
        public static readonly Dictionary<int, NatureData> TilesToData = new()
        {
            // Overworld: Sunflower-friendly
            [TileID.Grass] = new NatureData((NatureTags.Temp, 30),
        (NatureTags.Height, 50), (NatureTags.Moist, 40), (NatureTags.Good, 50)),

            [TileID.Dirt] = new NatureData((NatureTags.Temp, 25),
        (NatureTags.Height, 45), (NatureTags.Moist, 35), (NatureTags.Good, 40)),

            // Stone → ближче до Deadflower/Obsidianflower (холодно, низька якість)
            [TileID.Stone] = new NatureData((NatureTags.Temp, -20),
        (NatureTags.Height, -20), (NatureTags.Moist, 20), (NatureTags.Good, -20)),

            // Moss → вологіший камінь, трохи тепліший, але все ще бідний
            [TileID.GreenMoss] = new NatureData((NatureTags.Temp, -10),
        (NatureTags.Height, -30), (NatureTags.Moist, 45), (NatureTags.Good, -10)),
            [TileID.BlueMoss] = new NatureData((NatureTags.Temp, -20),
        (NatureTags.Height, -30), (NatureTags.Moist, 55), (NatureTags.Good, -10)),
            [TileID.ArgonMoss] = new NatureData((NatureTags.Temp, -5),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -5)),
            [TileID.BrownMoss] = new NatureData((NatureTags.Temp, -5),
        (NatureTags.Height, -30), (NatureTags.Moist, 45), (NatureTags.Good, -5)),
            [TileID.KryptonMoss] = new NatureData((NatureTags.Temp, -5),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -5)),
            [TileID.PurpleMoss] = new NatureData((NatureTags.Temp, -10),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -10)),
            [TileID.RainbowMoss] = new NatureData((NatureTags.Temp, 0),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -5)),
            [TileID.RedMoss] = new NatureData((NatureTags.Temp, 5),
        (NatureTags.Height, -30), (NatureTags.Moist, 45), (NatureTags.Good, -5)),
            [TileID.VioletMoss] = new NatureData((NatureTags.Temp, -5),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -10)),
            [TileID.XenonMoss] = new NatureData((NatureTags.Temp, -10),
        (NatureTags.Height, -30), (NatureTags.Moist, 50), (NatureTags.Good, -10)),

            // LavaMoss → ближче до Fireflower (гарячий, сухуватий, але на камені)
            [TileID.LavaMoss] = new NatureData((NatureTags.Temp, 70),
        (NatureTags.Height, -40), (NatureTags.Moist, 20), (NatureTags.Good, -5)),

            // Desert: Dryflower-friendly
            [TileID.Sand] = new NatureData((NatureTags.Temp, 65),
        (NatureTags.Height, -10), (NatureTags.Moist, -30), (NatureTags.Good, 10)),
            [TileID.HardenedSand] = new NatureData((NatureTags.Temp, 70),
        (NatureTags.Height, -20), (NatureTags.Moist, -40), (NatureTags.Good, 0)),
            [TileID.Sandstone] = new NatureData((NatureTags.Temp, 75),
        (NatureTags.Height, -30), (NatureTags.Moist, -50), (NatureTags.Good, -10)),

            // Snow/Ice: Snowflower & Iceflower-friendly
            [TileID.SnowBlock] = new NatureData((NatureTags.Temp, -40),
        (NatureTags.Height, 60), (NatureTags.Moist, 50), (NatureTags.Good, 40)),
            [TileID.IceBlock] = new NatureData((NatureTags.Temp, -80),
        (NatureTags.Height, -60), (NatureTags.Moist, 60), (NatureTags.Good, 20)),

            // Jungle: Sporeflower-friendly
            [TileID.JungleGrass] = new NatureData((NatureTags.Temp, 60),
        (NatureTags.Height, 45), (NatureTags.Moist, 70), (NatureTags.Good, 20)),
            [TileID.Mud] = new NatureData((NatureTags.Temp, 50),
        (NatureTags.Height, 35), (NatureTags.Moist, 50), (NatureTags.Good, 10)),

            // Underworld: Fireflower-friendly
            [TileID.Ash] = new NatureData((NatureTags.Temp, 90),
        (NatureTags.Height, -70), (NatureTags.Moist, -60), (NatureTags.Good, -10)),
            [TileID.Hellstone] = new NatureData((NatureTags.Temp, 100),
        (NatureTags.Height, -90), (NatureTags.Moist, -70), (NatureTags.Good, -20)),
            [TileID.AshGrass] = new NatureData((NatureTags.Temp, 95),
        (NatureTags.Height, -80), (NatureTags.Moist, -65), (NatureTags.Good, -15)),

            // Deadflower / Obsidianflower
            [TileID.Obsidian] = new NatureData((NatureTags.Temp, -60),
        (NatureTags.Height, -70), (NatureTags.Moist, -50), (NatureTags.Good, -40)),

            // Clouds (високо, холодно, мало користі)
            [TileID.Cloud] = new NatureData((NatureTags.Temp, -10),
        (NatureTags.Height, 100), (NatureTags.Moist, -20), (NatureTags.Good, 10)),
            [TileID.RainCloud] = new NatureData((NatureTags.Temp, 0),
        (NatureTags.Height, 100), (NatureTags.Moist, 70), (NatureTags.Good, 20)),
            [TileID.SnowCloud] = new NatureData((NatureTags.Temp, -40),
        (NatureTags.Height, 100), (NatureTags.Moist, 60), (NatureTags.Good, 15)),

            // Ocean: Beachflower & Oceanflower-friendly
            [TileID.Coralstone] = new NatureData((NatureTags.Temp, 25),
        (NatureTags.Height, -30), (NatureTags.Moist, 90), (NatureTags.Good, 40)),
            [TileID.ShellPile] = new NatureData((NatureTags.Temp, 35),
        (NatureTags.Height, -20), (NatureTags.Moist, 50), (NatureTags.Good, 30)),
        };
        public static readonly Dictionary<DepthZone, NatureData> DepthZoneToData = new()
        {
            [DepthZone.Sky] = new NatureData((NatureTags.Temp, -20),
                (NatureTags.Height, 100), (NatureTags.Moist, -10), (NatureTags.Good, 0)),

            [DepthZone.Overworld] = new NatureData((NatureTags.Temp, 30),
                (NatureTags.Height, 50), (NatureTags.Moist, 10), (NatureTags.Good, 50)),

            [DepthZone.DirtLayer] = new NatureData((NatureTags.Temp, 15),
                (NatureTags.Height, 30), (NatureTags.Moist, 30), (NatureTags.Good, 30)),

            [DepthZone.RockLayer] = new NatureData((NatureTags.Temp, 0),
                (NatureTags.Height, -20), (NatureTags.Moist, 0), (NatureTags.Good, 10)),

            [DepthZone.Underworld] = new NatureData((NatureTags.Temp, 100),
                (NatureTags.Height, -100), (NatureTags.Moist, -70), (NatureTags.Good, -20)),
        };
        public static readonly NatureData OceanZoneToData = new NatureData(
        (NatureTags.Moist, 20),
        (NatureTags.Good, 5));
        public static readonly Dictionary<TypeOfSunflower, NatureData> TypeOfSunflowerToData = new()
        {
            [TypeOfSunflower.Sunflower] = new NatureData(
        (NatureTags.Moist, 48),
        (NatureTags.Height, 60),
        (NatureTags.Temp, 40),
        (NatureTags.Good, 60)
    ),

            [TypeOfSunflower.Dryflower] = new NatureData(
        (NatureTags.Temp, 72),
        (NatureTags.Height, 2),
        (NatureTags.Moist, -30),
        (NatureTags.Good, 22)
    ),

            [TypeOfSunflower.Fireflower] = new NatureData(
        (NatureTags.Temp, 100),
        (NatureTags.Height, -100),
        (NatureTags.Moist, -88),
        (NatureTags.Good, -23)
    ),

            [TypeOfSunflower.Snowflower] = new NatureData(
        (NatureTags.Temp, -30),
        (NatureTags.Height, 74),
        (NatureTags.Moist, 50),
        (NatureTags.Good, 50)
    ),

            [TypeOfSunflower.Iceflower] = new NatureData(
        (NatureTags.Temp, -80),
        (NatureTags.Moist, 62),
        (NatureTags.Height, -67),
        (NatureTags.Good, 20)
    ),

            [TypeOfSunflower.Beachflower] = new NatureData(
        (NatureTags.Temp, 56),
        (NatureTags.Moist, 70),
        (NatureTags.Height, 46),
        (NatureTags.Good, 27)
    ),

            [TypeOfSunflower.Oceanflower] = new NatureData(
        (NatureTags.Temp, 26),
        (NatureTags.Moist, 100),
        (NatureTags.Height, -37),
        (NatureTags.Good, 45)
    ),

            [TypeOfSunflower.Sporeflower] = new NatureData(
        (NatureTags.Temp, 66),
        (NatureTags.Moist, 78),
        (NatureTags.Height, 54),
        (NatureTags.Good, 26)
    ),

            [TypeOfSunflower.Deadflower] = new NatureData(
        (NatureTags.Temp, -30),
        (NatureTags.Moist, -20),
        (NatureTags.Height, -30),
        (NatureTags.Good, -100)
    ),

            [TypeOfSunflower.Obsidianflower] = new NatureData(
        (NatureTags.Temp, -70),
        (NatureTags.Moist, -70),
        (NatureTags.Height, -80),
        (NatureTags.Good, -50)
    ),
        };
        public static readonly Dictionary<TypeOfSunflower, int> TypeOfSunflowerToSeedItemId = new()
        {
            [TypeOfSunflower.Sunflower] = ModContent.ItemType<SunflowerSeed>(),
            [TypeOfSunflower.Dryflower] = ModContent.ItemType<DryflowerSeed>(),
            [TypeOfSunflower.Fireflower] = ModContent.ItemType<FireflowerSeed>(),
            [TypeOfSunflower.Snowflower] = ModContent.ItemType<SnowflowerSeed>(),
            [TypeOfSunflower.Iceflower] = ModContent.ItemType<IceflowerSeed>(),
            [TypeOfSunflower.Beachflower] = ModContent.ItemType<BeachflowerSeed>(),
            [TypeOfSunflower.Oceanflower] = ModContent.ItemType<OceanflowerSeed>(),
            [TypeOfSunflower.Sporeflower] = ModContent.ItemType<SporeflowerSeed>(),
            [TypeOfSunflower.Deadflower] = ModContent.ItemType<DeadflowerSeed>(),
            [TypeOfSunflower.Obsidianflower] = ModContent.ItemType<ObsidianflowerSeed>(),
        };
        public static readonly Dictionary<TypeOfSunflower, int> TypeOfSunflowerToItemId = new()
        {
            [TypeOfSunflower.Sunflower] = ItemID.Sunflower,
            [TypeOfSunflower.Dryflower] = ModContent.ItemType<Dryflower>(),
            [TypeOfSunflower.Fireflower] = ModContent.ItemType<Fireflower>(),
            [TypeOfSunflower.Snowflower] = ModContent.ItemType<Snowflower>(),
            [TypeOfSunflower.Iceflower] = ModContent.ItemType<Iceflower>(),
            [TypeOfSunflower.Beachflower] = ModContent.ItemType<Beachflower>(),
            [TypeOfSunflower.Oceanflower] = ModContent.ItemType<Oceanflower>(),
            [TypeOfSunflower.Sporeflower] = ModContent.ItemType<Sporeflower>(),
            [TypeOfSunflower.Deadflower] = ModContent.ItemType<Deadflower>(),
            [TypeOfSunflower.Obsidianflower] = ModContent.ItemType<Obsidianflower>(),
        };

        private readonly Dictionary<string, int> loves = new();

        public NatureData() { }

        public NatureData(params (string key, int value)[] entries)
        {
            foreach (var (key, value) in entries)
                loves[key] = value;
        }

        public int this[string key]
        {
            get
            {
                if (loves.TryGetValue(key, out var v))
                {
                    return v;
                }
                else
                {
                    loves[key] = 0;
                    return 0;
                }
            }
            set => loves[key] = value;
        }

        public IEnumerable<string> Keys => loves.Keys;
        public IEnumerable<int> Values => loves.Values;
        public IReadOnlyDictionary<string, int> ValuesMap => loves;

        public static NatureData operator +(NatureData a, NatureData b)
        {
            var result = new NatureData();
            foreach (var key in AllKeys(a, b))
                result[key] = a[key] + b[key];
            return result;
        }

        public static NatureData operator -(NatureData a, NatureData b)
        {
            var result = new NatureData();
            foreach (var key in AllKeys(a, b))
                result[key] = a[key] - b[key];
            return result;
        }

        public static NatureData operator *(NatureData a, float multiplier)
        {
            var result = new NatureData();
            foreach (var kvp in a.loves)
                result[kvp.Key] = (int)Math.Round(kvp.Value * multiplier);
            return result;
        }

        public static NatureData operator /(NatureData a, float divisor)
        {
            var result = new NatureData();
            foreach (var kvp in a.loves)
                result[kvp.Key] = (int)Math.Round(kvp.Value / divisor);
            return result;
        }

        public float TotalLove => loves.Values.Sum();

        public float AverageLove => TotalLove / NatureTags.Count;

        private static HashSet<string> AllKeys(NatureData a, NatureData b)
        {
            var keys = new HashSet<string>(a.loves.Keys);
            keys.UnionWith(b.loves.Keys);
            return keys;
        }

        public override string ToString()
        {
            return string.Join(", ", loves.Select(kv => $"{kv.Key}: {kv.Value}"));
        }

        public NatureData Normalize()
        {
            NatureData normalized = new NatureData();
            foreach (var key in loves.Keys.ToList())
            {
                normalized[key] = Math.Clamp(loves[key], -100, 100); // Ensure values are between 0 and 100
            }
            return normalized;
        }

        public NatureData Abs()
        {
            NatureData absolute = new NatureData();
            foreach (var key in loves.Keys.ToList())
            {
                absolute[key] = Math.Abs(loves[key]);
            }
            return absolute;
        }

        public NatureData Clone()
        {
            NatureData clone = new NatureData();
            foreach (var kvp in loves)
            {
                clone[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public float DistanceTo(NatureData other)
        {
            float sum = 0f;

            foreach (var tag in NatureTags.AllTags)
            {
                float a = this[tag];
                float b = other[tag];
                float diff = a - b;
                sum += diff * diff;
            }

            return MathF.Sqrt(sum);
        }

        public TypeOfSunflower FindClosestTypeOfSunflower()
        {
            TypeOfSunflower closestType = default;
            float minDistance = float.MaxValue;

            foreach (var pair in NatureData.TypeOfSunflowerToData)
            {
                float dist = DistanceTo(pair.Value);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestType = pair.Key;
                }
            }
            return closestType;
        }
    }
    public static class NatureTags
    {
        public const string Moist = "Moist";
        public const string Height = "Height";
        public const string Temp = "Temp";
        public const string Good = "Good";
        public static string[] AllTags =
        [
            Moist, Height, Temp, Good
        ];
        public static int Count => AllTags.Count(); // Total number of tags, used for validation
    }
    public enum TypeOfSunflower
    {
        None = -1,
        Sunflower = 0,
        Dryflower = 1,
        Fireflower = 2,
        Snowflower = 3,
        Iceflower = 4,
        Beachflower = 5,
        Oceanflower = 6,
        Sporeflower = 7,
        Deadflower = 8,
        Obsidianflower = 9,
    }
}

