using Humanizer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Sunflowerology.Common.Configs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth
{
    public abstract class PlantStageTile<T> : ModTile where T : FinalPlantStageEntity
    {
        protected abstract int WidthInTiles { get; }
        protected abstract int HeightInTiles { get; }
        protected abstract bool IsSpecialHook { get; }
        protected abstract bool HaveGlow { get; }
        protected abstract int[] Heights { get; }

        protected Asset<Texture2D> glowTexture = null;

        protected virtual string GetmouseOverPlantText(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out T tileEntity))
            {
                string res = $"Growth: {(int)tileEntity.growthLevel}";
                /*string res = $"Growth: {(int)tileEntity.growthLevel}, Diff: {tileEntity.averageDifference:F2}";
foreach (var seedTag in NatureTags.AllTags)
{
    res += $"\n{seedTag}: {tileEntity.plantData[seedTag]}, " +
        $"S:{tileEntity.surroundingAreaData[seedTag]}, " +
        $"D: {tileEntity.difference[seedTag]}";
}
                 */
                return res;
            }
            else
            {
                return "No tile entity found.";
            }
        }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLighted[Type] = true;

            DustType = DustID.Grass;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = WidthInTiles;
            TileObjectData.newTile.Height = HeightInTiles;
            TileObjectData.newTile.CoordinateHeights = Heights;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.StyleWrapLimit = 3;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.AnchorValidTiles = NatureData.TilesToData.Keys.ToArray();
            if (IsSpecialHook)
            {
                //TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<T>().Hook_AfterPlacement, -1, 0, true);
            }
            else
            {
                TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<T>().Generic_HookPostPlaceMyPlayer;
            }
            TileObjectData.addTile(Type);

            if (HaveGlow)
            {
                glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
            }
            AddMapEntry(new Color(230, 230, 0));
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (glowTexture == null || !HaveGlow)
            {
                return;
            }
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            spriteBatch.Draw(
                glowTexture.Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            var tileData = TileObjectData.GetTileData(Type, 0);
            if (tileData.Height == 1 && tileData.Width == 1)
            {
                return;
            }

            int id = ModContent.GetInstance<T>().Find(i, j);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {

                ModContent.GetInstance<T>().Kill(i, j);
                NetMessage.SendData(MessageID.TileEntitySharing, number: id);
            }
            else
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: id);
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (effectOnly || fail)
            {
                return;
            }
            var tileData = TileObjectData.GetTileData(Type, 0);
            if (tileData.Height != 1 || tileData.Width != 1)
            {
                return;
            }

            int id = ModContent.GetInstance<T>().Find(i, j);

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {

                ModContent.GetInstance<T>().Kill(i, j);
                NetMessage.SendData(MessageID.TileEntitySharing, number: id);
            }
            else
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: id);
            }

        }

        //TODO: Make this more beautifull
        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = -1;
            player.cursorItemIconText = GetmouseOverPlantText(i, j);
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.1f;
            b = 0.1f;
        }


    }

    public abstract class FinalPlantStageEntity : ModTileEntity
    {
        public float growthLevel
        {
            get;
            set
            {
                field = Math.Clamp(value, 0f, 100f);
            }
        } = 0;

        // Data of the surrounding area (calculated from the tiles around the plant)
        public NatureData surroundingAreaData = new NatureData();
        public NatureData difference = new();
        public float averageDifference = 0f;
        public TypeOfSunflower typeOfSunflower = TypeOfSunflower.None;

        // Data of the plant itself (not changing)
        public NatureData plantData = new NatureData();
        protected abstract int TileType { get; }

        public bool IsDead => typeOfSunflower == TypeOfSunflower.Deadflower;

        // Technical variables
        protected int updateCounter = 0;
        protected int PlantUpdateInterval => Conf.C.PlantUpdateInterval;
        protected bool SkippedUpdate => updateCounter != 1;

        public void SetUpData(NatureData nt)
        {
            plantData = nt.Clone();
        }

        // ModTileEntity's methods
        public override bool IsTileValidForEntity(int x, int y)
        {
            return Main.tile[x, y].HasTile && Main.tile[x, y].TileType == TileType;
        }
        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                tag[seedTag] = plantData[seedTag];
            }
            tag[nameof(typeOfSunflower)] = (int)typeOfSunflower;
        }
        public override void LoadData(TagCompound tag)
        {
            var newPlantData = new NatureData();
            foreach (var seedTag in NatureTags.AllTags)
            {
                try
                {
                    if (tag.TryGet(seedTag, out int val))
                    {
                        newPlantData[seedTag] = val;
                    }
                    else
                    {
                        newPlantData[seedTag] = 0;
                    }
                }
                catch
                {
                    newPlantData[seedTag] = 0;
                }
            }
            plantData = newPlantData;

            if (tag.TryGet(nameof(typeOfSunflower), out int type))
            {
                typeOfSunflower = (TypeOfSunflower)type;
            }
            else
            {
                typeOfSunflower = TypeOfSunflower.None;
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                writer.Write(plantData[seedTag]);
            }
            writer.Write((int)typeOfSunflower);
        }
        public override void NetReceive(BinaryReader reader)
        {
            var newPlantData = new NatureData();
            foreach (var seedTag in NatureTags.AllTags)
            {
                newPlantData[seedTag] = reader.ReadInt32();
            }
            plantData = newPlantData;
            typeOfSunflower = (TypeOfSunflower)reader.ReadInt32();

        }

        public override void Update()
        {
            if (typeOfSunflower == TypeOfSunflower.None && !IsDead)
            {
                typeOfSunflower = plantData.FindClosestTypeOfSunflower();
            }
            if (SkipUpdate()) return;
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
        }

        protected bool SkipUpdate()
        {
            updateCounter++;
            updateCounter %= PlantUpdateInterval;
            return SkippedUpdate;
        }

    }

    public abstract class PlantStageEntity<E> : FinalPlantStageEntity where E : FinalPlantStageEntity
    {

        protected abstract int NextTileType { get; }
        protected virtual bool IsFullyGrown => growthLevel >= 100;
        public int HeightInTiles => TileObjectData.GetTileData(TileType, 0).Height;

        protected static List<(int X, int Y, NatureData Data, NatureData Surround, bool isdead)> growthQueue = new();

        // ModTileEntity's methods
        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag[nameof(growthLevel)] = growthLevel;
        }
        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.TryGet(nameof(growthLevel), out float gl))
            {
                growthLevel = gl;
            }
            else
            {
                growthLevel = 0f;
            }
        }
        public override void NetSend(BinaryWriter writer)
        {
            base.NetSend(writer);
            writer.Write(growthLevel);

        }
        public override void NetReceive(BinaryReader reader)
        {
            base.NetReceive(reader);
            growthLevel = reader.ReadSingle();
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

            // Calculate the surrounding area data, difference, and average difference
            surroundingAreaData = CalculateSurroundings();
            difference = surroundingAreaData - plantData;
            averageDifference = CalculateAverageDifference();

            // Calculate the growth level
            growthLevel += CalculateGrowth();

            // Check if the plant is fully grown
            if (IsFullyGrown)
                ReplacePlantWithNewOne();

            // Send the updated data to the server
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
        }

        public override void PostGlobalUpdate()
        {
            if (growthQueue.Count == 0 || NextTileType == -1) return;

            GrowNewPlant();
        }

        protected E GetEntityOn(int i, int j)
        {
            int id = ModContent.GetInstance<E>().Place(i, j);
            if (TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is E e)
            {
                return e;
            }
            return default;
        }
        protected virtual void GrowNewPlant()
        {
            if (NextTileType == -1)
            {
                return;
            }
            Random random = new Random();

            // Going through all the plants that need to be grown
            foreach (var (i, j, plantD, errorPD, isdead) in growthQueue)
            {
                // Creaating mutation of the plant data
                var newPlantData = plantD.Clone();
                if (!isdead)
                {
                    foreach (string seedTag in NatureTags.AllTags)
                    {
                        int randomInt = random.Next(0, 10);
                        float change = 0f;

                        if (randomInt == 0)
                        {
                            // Make the plant grow with a bit of error
                            change = -(float)errorPD[seedTag] * 2f / 3f;
                        }
                        else if (randomInt > 0 && randomInt < 9)
                        {
                            // Make the plant grow with a lesser error
                            change = (float)errorPD[seedTag] / 3f;
                        }
                        else if (randomInt == 9)
                        {
                            // Make the plant grow with even lesser error
                            change = (float)errorPD[seedTag] * 3 / 4;
                        }

                        change = Math.Clamp(change, -10, 10);

                        newPlantData[seedTag] = (int)Math.Round(plantD[seedTag] + change);
                    }
                }

                newPlantData = newPlantData.Normalize();

                // Finding the closest type of sunflower based on the new plant data
                var newTypeOfSunflower = (int)newPlantData.FindClosestTypeOfSunflower();
                if (isdead)
                {
                    newTypeOfSunflower = (int)TypeOfSunflower.Deadflower;
                }

                // Random style for the new plant
                int randomStyle = random.Next(0, 3);
                int style = 3 * newTypeOfSunflower;

                // Place the new tile
                if (!PlaceNextTile(i, j, newPlantData, randomStyle, style))
                {
                    continue;
                }
            }
            growthQueue.Clear();
        }

        protected virtual bool PlaceNextTile(int i, int j, NatureData newPlantData, int randomStyle, int style)
        {
            if (!WorldGen.PlaceObject(i, j, NextTileType, mute: true, style: style, random: randomStyle))
            {
                return false;
            }
            //var tod = TileObjectData.GetTileData(NextTileType, style);
            var point = TileObjectData.TopLeft(i, j);
            var newEntity = GetEntityOn(point.X, point.Y);

            newEntity.plantData = newPlantData;
            if (IsDead)
            {
                newEntity.typeOfSunflower = TypeOfSunflower.Deadflower;
            }
            NetMessage.SendObjectPlacement(-1, i, j, NextTileType, style, 0, randomStyle, -1);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: newEntity.ID);
            return true;
        }

        protected virtual void ReplacePlantWithNewOne()
        {
            var tileAboveLeft = Framing.GetTileSafely(Position.X, Position.Y - 1);
            var tileAboveRight = Framing.GetTileSafely(Position.X + 1, Position.Y - 1);
            if (tileAboveLeft.HasTile || tileAboveRight.HasTile)
            {
                return;
            }
            WorldGen.KillTile(Position.X, Position.Y, false, false, true);
            Kill(Position.X, Position.Y);

            growthQueue.Add((Position.X, Position.Y - 1, plantData.Clone(), difference.Clone(), IsDead));

            NetMessage.SendTileSquare(-1, Position.X, Position.Y, 2, 4);
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID);
        }

        protected virtual float CalculateGrowth()
        {
            if (Main.rand.NextBool((int)Math.Ceiling(Math.Pow(averageDifference, 2) / 10) + 2) || Conf.C.GrowFast)
            {
                // Chance to become a deadflower if the average difference is too high
                DeadflowerChance();

                // Conf.C.GrowFast invokes CalculateGrowth 10 times per update instead of 1
                if (Conf.C.GrowFast)
                {
                    for (int k = 0; k < 9; k++)
                    {
                        DeadflowerChance();
                    }
                    return PlantUpdateInterval / 3f;
                }

                // Return normal growth
                return PlantUpdateInterval / 30f;
            }
            return 0f;
        }

        private void DeadflowerChance()
        {
            if (!IsDead && Main.rand.NextBool(100 - (int)averageDifference) && Main.rand.NextBool(1000))
            {
                typeOfSunflower = TypeOfSunflower.Deadflower;
                plantData[NatureTags.Good] = -100 + Main.rand.Next(0, 10);
                plantData[NatureTags.Moist] += -50 + Main.rand.Next(0, 20);
                plantData[NatureTags.Temp] += -50 + Main.rand.Next(0, 20);
                plantData[NatureTags.Height] += -50 + Main.rand.Next(0, 20);
                plantData = plantData.Normalize();
            }
        }

        protected NatureData CalculateSurroundings()
        {
            var saData = new NatureData();
            saData = CalculateSATiles(saData);
            saData = CalculateSADepth(saData);
            saData = CalculateSAOcean(saData);
            saData = saData.Normalize();
            return saData;
        }

        protected NatureData CalculateSAOcean(NatureData saData)
        {
            if (IsOcean(Position.X))
            {
                saData += NatureData.OceanZoneToData;
            }

            return saData;
        }

        public static bool IsOcean(int tileX)
        {
            return tileX < 380 || tileX > Main.maxTilesX - 380;
        }

        protected NatureData CalculateSADepth(NatureData saData)
        {
            if (NatureData.DepthZoneToData.TryGetValue(GetDepthZone(Position.Y), out var i))
            {
                saData += i * 0.25f;
            }

            return saData;
        }

        protected NatureData CalculateSATiles(NatureData saData)
        {
            Tile[] Tiles25p = [Framing.GetTileSafely(Position.X, Position.Y + HeightInTiles),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + HeightInTiles)];
            Tile[] Tiles125p = [Framing.GetTileSafely(Position.X, Position.Y + HeightInTiles + 1),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + HeightInTiles + 1)];///FINISH THIS
            Tile[] Tiles625p = [Framing.GetTileSafely(Position.X - 1, Position.Y + HeightInTiles),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + HeightInTiles),
                                Framing.GetTileSafely(Position.X - 1, Position.Y + HeightInTiles + 1),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + HeightInTiles + 1),];
            foreach (Tile tile in Tiles25p)
            {
                if (tile.HasTile && NatureData.TilesToData.TryGetValue(tile.TileType, out NatureData seedData))
                {
                    saData += seedData * 0.25f;
                }
            }
            foreach (Tile tile in Tiles125p)
            {
                if (tile.HasTile && NatureData.TilesToData.TryGetValue(tile.TileType, out NatureData seedData))
                {
                    saData += seedData * 0.125f;
                }
            }
            foreach (Tile tile in Tiles625p)
            {
                if (tile.HasTile && NatureData.TilesToData.TryGetValue(tile.TileType, out NatureData seedData))
                {
                    saData += seedData * 0.0625f;
                }
            }

            return saData;
        }

        protected float CalculateAverageDifference()
        {
            var diff = difference.Clone();
            diff = diff.Abs();
            return diff.AverageLove;
        }
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

