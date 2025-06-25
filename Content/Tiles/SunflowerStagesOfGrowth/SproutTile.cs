using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ScienceJam.Common.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SproutTile : PlantStageTile<SproutEntity>
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            var tileData = TileObjectData.GetTileData(Type, 0);
            tileData.AnchorValidTiles = NatureData.BlocksToSeedsProperties.Keys.ToArray();
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            if (TileEntity.TryGet(i, j, out SproutEntity tileEntity))
            {
                if (!tileEntity.updated)
                {
                    tileEntity.Update();
                }
                tileFrameY += (short)(20 * 3 * (int)tileEntity.typeOfSunflower);
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

        protected override bool IsSpecialHook => true;

        protected override bool HaveGlow => false;

        protected override int[] Heights => [18];
    }

    public class SproutEntity : PlantStageEntity<SeedlingEntity>
    {
        //Sprout Data
        public static NatureData transferData = new NatureData();
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
            if (SkipUpdate()) return;
            if (!IsPaired)
            {
                // Check for unpaired sprouts on both sides
                if (!CheckAndPairUnpairedSprout(PairingState.OnRight))
                {
                    CheckAndPairUnpairedSprout(PairingState.OnLeft);
                }
            }
            else if (Pairing == PairingState.OnRight)
            {
                ModContent.GetModTile(NextTileType);
                surroundingAreaData = CalculateSurroundings();
                difference = surroundingAreaData - ((plantData + pairedEntity.plantData) / 2);
                pairedEntity.difference = difference;
                averageDifference = CalculateAverageDifference();
                pairedEntity.averageDifference = averageDifference;
                typeOfSunflower = plantData.FindClosestTypeOfSunflower();
                growthLevel += CalculateGrowth();

                if (IsFullyGrown && pairedEntity.IsFullyGrown)
                    ReplacePlantWithNewOne();
            }
            else if (Pairing == PairingState.OnLeft)
            {
                typeOfSunflower = plantData.FindClosestTypeOfSunflower();
                growthLevel += CalculateGrowth();
            }
            updated = true;
        }

        protected override void ReplacePlantWithNewOne()
        {
            base.ReplacePlantWithNewOne();
            WorldGen.KillTile(pairedEntity.Position.X, pairedEntity.Position.Y, false, false, true);
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
                    entity.plantData = transferData;
                    transferData = new NatureData(); // Clear the transfer data after placing
                }
                return -1;
            }
            int result = Place(point.X, point.Y);
            if (TileEntity.TryGet(point.X, point.Y, out SproutEntity tileEntity))
            {
                tileEntity.plantData = transferData;
                transferData = new NatureData(); // Clear the transfer data after placing
            }
            return result;
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
        public static readonly Dictionary<int, NatureData> BlocksToSeedsProperties = new()
        {
            // Overworld blocks and their variations
            [TileID.Grass] = new NatureData(
        (NatureTags.Sun, 100), (NatureTags.Water, 40), (NatureTags.Wild, 30), (NatureTags.Dry, 10)),
            [TileID.Dirt] = new NatureData(
        (NatureTags.Sun, 70), (NatureTags.Dry, 50), (NatureTags.Water, 20), (NatureTags.Cave, 10)),

            // Stone and moss blocks
            [TileID.Stone] = new NatureData(
        (NatureTags.Cave, 100), (NatureTags.Dry, 60), (NatureTags.Cold, 30)),
            [TileID.GreenMoss] = new NatureData(
        (NatureTags.Cave, 75), (NatureTags.Water, 65), (NatureTags.Wild, 60)),
            [TileID.BlueMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Cold, 40)),
            [TileID.ArgonMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Good, 40)),
            [TileID.BrownMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Dry, 20)),
            [TileID.KryptonMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 70), (NatureTags.Wild, 90)),
            [TileID.PurpleMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Evil, 50)),
            [TileID.RainbowMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Evil, 20), (NatureTags.Good, 20), (NatureTags.Honey, 50)),
            [TileID.RedMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Evil, 50)),
            [TileID.VioletMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Water, 30)),
            [TileID.XenonMoss] = new NatureData(
        (NatureTags.Cave, 70), (NatureTags.Water, 80), (NatureTags.Cold, 30)),
            [TileID.LavaMoss] = new NatureData(
        (NatureTags.Cave, 85), (NatureTags.Hot, 70), (NatureTags.Dry, 50)),

            // Dry and sandy blocks
            [TileID.Sand] = new NatureData(
        (NatureTags.Dry, 100), (NatureTags.Hot, 80), (NatureTags.Sun, 60), (NatureTags.Water, 10)),
            [TileID.HardenedSand] = new NatureData(
        (NatureTags.Dry, 90), (NatureTags.Hot, 70), (NatureTags.Cave, 40)),
            [TileID.Sandstone] = new NatureData(
        (NatureTags.Dry, 80), (NatureTags.Cave, 60), (NatureTags.Hot, 50)),

            // Snow and ice blocks
            [TileID.SnowBlock] = new NatureData(
        (NatureTags.Cold, 100), (NatureTags.Sun, 60), (NatureTags.Water, 40)),
            [TileID.IceBlock] = new NatureData(
        (NatureTags.Cold, 100), (NatureTags.Cave, 50), (NatureTags.Water, 80)),

            // Jungle blocks and mud
            [TileID.JungleGrass] = new NatureData(
        (NatureTags.Wild, 100), (NatureTags.Water, 100), (NatureTags.Sun, 50), (NatureTags.Hot, 30)),
            [TileID.Mud] = new NatureData(
        (NatureTags.Wild, 80), (NatureTags.Water, 100), (NatureTags.Sun, 20)),

            // Hallowed blocks and their variations
            [TileID.HallowedGrass] = new NatureData(
        (NatureTags.Good, 100), (NatureTags.Sun, 80), (NatureTags.Water, 30), (NatureTags.Wild, 20)),
            [TileID.Pearlstone] = new NatureData(
        (NatureTags.Good, 90), (NatureTags.Cave, 70), (NatureTags.Water, 20)),
            [TileID.Pearlsand] = new NatureData(
        (NatureTags.Good, 80), (NatureTags.Sun, 60), (NatureTags.Dry, 50)),
            [TileID.HallowedIce] = new NatureData(
        (NatureTags.Good, 100), (NatureTags.Cold, 80), (NatureTags.Water, 50)),

            // Evil blocks and their variations
            [TileID.CorruptGrass] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Sun, 30), (NatureTags.Dry, 40), (NatureTags.Water, 10)),
            [TileID.Ebonstone] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Cave, 60), (NatureTags.Dry, 40)),
            [TileID.Crimstone] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Cave, 60), (NatureTags.Dry, 40)),
            [TileID.CrimsonGrass] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Sun, 30), (NatureTags.Water, 30), (NatureTags.Hot, 20)),
            [TileID.Crimsand] = new NatureData(
        (NatureTags.Evil, 90), (NatureTags.Sun, 50), (NatureTags.Dry, 60), (NatureTags.Hot, 20)),
            [TileID.Ebonsand] = new NatureData(
        (NatureTags.Evil, 90), (NatureTags.Sun, 50), (NatureTags.Dry, 60), (NatureTags.Hot, 20)),
            [TileID.FleshIce] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Cold, 80), (NatureTags.Water, 50), (NatureTags.Dry, 20)),
            [TileID.CorruptIce] = new NatureData(
        (NatureTags.Evil, 100), (NatureTags.Cold, 80), (NatureTags.Water, 50), (NatureTags.Dry, 20)),

            // Hell blocks and their variations
            [TileID.Ash] = new NatureData(
        (NatureTags.Hot, 100), (NatureTags.Dry, 100), (NatureTags.Cave, 80), (NatureTags.Evil, 20)),
            [TileID.Hellstone] = new NatureData(
        (NatureTags.Hot, 100), (NatureTags.Cave, 90), (NatureTags.Dry, 80), (NatureTags.Evil, 50)),
            [TileID.AshGrass] = new NatureData(
        (NatureTags.Hot, 60), (NatureTags.Dry, 50), (NatureTags.Cave, 90), (NatureTags.Evil, 10)),
            [TileID.Obsidian] = new NatureData(
                (NatureTags.Hot, 100), (NatureTags.Cave, 80), (NatureTags.Cold, 100)),

            // Honey blocks and their variations
            [TileID.HoneyBlock] = new NatureData(
        (NatureTags.Honey, 100), (NatureTags.Water, 80), (NatureTags.Sun, 40), (NatureTags.Wild, 30)),
            [TileID.CrispyHoneyBlock] = new NatureData(
        (NatureTags.Honey, 90), (NatureTags.Dry, 70), (NatureTags.Sun, 50), (NatureTags.Hot, 40)),
            [TileID.BeeHive] = new NatureData(
        (NatureTags.Honey, 100), (NatureTags.Sun, 90), (NatureTags.Water, 60)),

            // Clouds and their variations
            [TileID.Cloud] = new NatureData(
        (NatureTags.Sun, 100), (NatureTags.Water, 90), (NatureTags.Cold, 30)),
            [TileID.RainCloud] = new NatureData(
        (NatureTags.Sun, 100), (NatureTags.Water, 100), (NatureTags.Cold, 40)),
            [TileID.SnowCloud] = new NatureData(
        (NatureTags.Sun, 100), (NatureTags.Cold, 100), (NatureTags.Water, 70)),

            // Ocean blocks and their variations
            [TileID.Coralstone] = new NatureData(
        (NatureTags.Water, 100), (NatureTags.Sun, 60), (NatureTags.Wild, 50)),
            [TileID.ShellPile] = new NatureData(
        (NatureTags.Water, 90), (NatureTags.Sun, 30), (NatureTags.Dry, 20)),
        };
        public static readonly Dictionary<DepthZone, NatureData> DepthZoneToSeedAffinity = new()
        {
            [DepthZone.Sky] = new NatureData((NatureTags.Sun, 100), (NatureTags.Cave, -100), (NatureTags.Cold, 20)),
            [DepthZone.Overworld] = new NatureData((NatureTags.Sun, 50), (NatureTags.Cave, -50)),
            [DepthZone.DirtLayer] = new NatureData((NatureTags.Cave, 20), (NatureTags.Sun, 30)),
            [DepthZone.RockLayer] = new NatureData((NatureTags.Cave, 80), (NatureTags.Hot, 10), (NatureTags.Sun, -50)),
            [DepthZone.Underworld] = new NatureData((NatureTags.Hot, 100), (NatureTags.Cave, 60), (NatureTags.Sun, -100)),
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
            get => loves.TryGetValue(key, out var v) ? v : 0;
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
                normalized[key] = Math.Clamp(loves[key], 0, 100); // Ensure values are between 0 and 100
            }
            return normalized;
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

            foreach (var pair in SunflowersPropertiesData.TypeToData)
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
    public enum TypeOfSunflower
    {
        Sunflower = 0,
        Dryflower = 1,
        Fireflower = 2,
        Snowflower = 3,
        Iceflower = 4,
        Beachflower = 5,
        Oceanflower = 6,
        Jungleflower = 7,
        Deadflower = 8,
        Obsidianflower = 9,
    }
    public static class SunflowersPropertiesData
    {
        public static Dictionary<TypeOfSunflower, NatureData> TypeToData = new()
        {
            [TypeOfSunflower.Sunflower] = new NatureData(
        (NatureTags.Dry, 30),
        (NatureTags.Water, 30),
        (NatureTags.Wild, 20),
        (NatureTags.Sun, 80),
        (NatureTags.Cave, 10),
        (NatureTags.Hot, 10),
        (NatureTags.Cold, 10),
        (NatureTags.Evil, 0),
        (NatureTags.Good, 0),
        (NatureTags.Honey, 0)
    ),

            [TypeOfSunflower.Dryflower] = new NatureData(
        (NatureTags.Dry, 100),
        (NatureTags.Hot, 80),
        (NatureTags.Sun, 60),
        (NatureTags.Cave, 30),
        (NatureTags.Water, 10),
        (NatureTags.Cold, 10)
    ),

            [TypeOfSunflower.Fireflower] = new NatureData(
        (NatureTags.Hot, 100),
        (NatureTags.Sun, 80),
        (NatureTags.Dry, 50),
        (NatureTags.Water, 20),
        (NatureTags.Cold, 10),
        (NatureTags.Cave, 40)
    ),

            [TypeOfSunflower.Snowflower] = new NatureData(
        (NatureTags.Cold, 100),
        (NatureTags.Sun, 60),
        (NatureTags.Water, 40),
        (NatureTags.Dry, 10),
        (NatureTags.Hot, 10),
        (NatureTags.Cave, 30)
    ),

            [TypeOfSunflower.Iceflower] = new NatureData(
        (NatureTags.Cold, 100),
        (NatureTags.Cave, 50),
        (NatureTags.Water, 80),
        (NatureTags.Hot, 10),
        (NatureTags.Sun, 30),
        (NatureTags.Good, 20)
    ),

            [TypeOfSunflower.Beachflower] = new NatureData(
        (NatureTags.Water, 100),
        (NatureTags.Sun, 60),
        (NatureTags.Wild, 50),
        (NatureTags.Dry, 30),
        (NatureTags.Honey, 20),
        (NatureTags.Cold, 10)
    ),

            [TypeOfSunflower.Oceanflower] = new NatureData(
        (NatureTags.Water, 100),
        (NatureTags.Sun, 60),
        (NatureTags.Wild, 50),
        (NatureTags.Dry, 10),
        (NatureTags.Good, 20),
        (NatureTags.Honey, 30)
    ),

            [TypeOfSunflower.Jungleflower] = new NatureData(
        (NatureTags.Wild, 100),
        (NatureTags.Water, 100),
        (NatureTags.Sun, 50),
        (NatureTags.Hot, 30),
        (NatureTags.Honey, 20),
        (NatureTags.Cave, 20)
    ),

            [TypeOfSunflower.Deadflower] = new NatureData(
        (NatureTags.Evil, 100),
        (NatureTags.Sun, 10),
        (NatureTags.Water, 10),
        (NatureTags.Good, 0),
        (NatureTags.Honey, 0),
        (NatureTags.Wild, 10)
    ),

            [TypeOfSunflower.Obsidianflower] = new NatureData(
        (NatureTags.Hot, 100),
        (NatureTags.Cold, 100),
        (NatureTags.Cave, 100),
        (NatureTags.Water, 40),
        (NatureTags.Dry, 20),
        (NatureTags.Evil, 10)
    ),
        };


    }
}

