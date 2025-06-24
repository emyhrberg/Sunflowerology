using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Utils;
using ScienceJam.Common.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SeedlingTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Grass;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<SeedlingEntity>().Generic_HookPostPlaceMyPlayer;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<SeedlingEntity>().Kill(i, j);
        }

        public override void MouseOver(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out SeedlingEntity tileEntity))
            {
                Player player = Main.LocalPlayer;
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = -1;
                player.cursorItemIconText = $"Growth: {tileEntity.growthAmount}, Diff: {tileEntity.seedSurroundingDifference}";
                foreach (var seedTag in SeedTags.AllTags)
                {
                    player.cursorItemIconText += $"\n{seedTag}: {tileEntity.seedData[seedTag]}, S:{tileEntity.surroundingAreaData[seedTag]}, D: " +
                        $"{tileEntity.seedSurroundingDifferenceDetailed[seedTag]}";
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (TileEntity.TryGet(i, j, out SeedlingEntity tileEntity))
            {
                Tile tile = Main.tile[i, j];
                Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
                try
                {
                    Texture2D texture = ModContent.Request<Texture2D>(Texture + "_" + tileEntity.typeOfSunflower.ToString()).Value;
                    if (texture == null || texture.IsDisposed)
                    {
                        Main.NewText($"Error: Texture for {tileEntity.typeOfSunflower.ToString()} not found or disposed.", Color.Red);
                        return true;
                    }
                    spriteBatch.Draw(
                        texture,
                        new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y) + zero,
                        new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, tile.TileFrameY >= 18 * 1 ? 18 : 16),
                        Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    return false;
                }
                catch
                {
                    Main.NewText($"Error: Texture for {tileEntity.typeOfSunflower.ToString()} not found.", Color.Red);
                    return true;
                }


            }
            return true;
        }
    }
    public class SeedlingEntity : ModTileEntity
    {
        public TypesOfSunflowers typeOfSunflower = TypesOfSunflowers.Sunflower;
        public TypesOfSunflowers FindClosestType()
        {
            TypesOfSunflowers closestType = default;
            float minDistance = float.MaxValue;

            foreach (var pair in SunflowersPropertiesData.TypeToData)
            {
                float dist = seedData.DistanceTo(pair.Value);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closestType = pair.Key;
                }
            }

            return closestType;
        }
        public SeedData seedData = new SeedData();
        public SeedData surroundingAreaData = new SeedData();
        public SeedData seedSurroundingDifferenceDetailed = new();
        public float seedSurroundingDifference = 0f;

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
        public static List<(int, int, SeedData, SeedData)> saplingPosToGrow = new();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SeedlingTile>();
        }
        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                tag[seedTag] = seedData[seedTag];
            }
        }
        public override void LoadData(TagCompound tag)
        {
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
            foreach (var seedTag in SeedTags.AllTags)
            {
                writer.Write(seedData[seedTag]);
            }

        }
        public override void NetReceive(BinaryReader reader)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                seedData[seedTag] = reader.ReadInt32();
            }
        }

        public override void PostGlobalUpdate()
        {
            foreach (var (i, j, sd, esd) in saplingPosToGrow)
            {
                GrowSapling(i, j, sd, esd);
            }
            saplingPosToGrow.Clear();
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
            typeOfSunflower = FindClosestType();
            CalculateSurroundings();
            CalculateDifference();

            if (Main.rand.NextBool((int)Math.Ceiling((seedSurroundingDifference * seedSurroundingDifference) / 10f) + 1) || Conf.C.SunflowerGrowFast)
            {
                growthAmount += Conf.C.SunflowerGrowFast ? 10 : 1;
            }
            CheckIfGrowenUp();
        }

        private void CalculateSurroundings()
        {
            surroundingAreaData = new SeedData();
            int n = 2;
            Tile[] Tiles25p = [Framing.GetTileSafely(Position.X, Position.Y + n),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + n)];
            Tile[] Tiles125p = [Framing.GetTileSafely(Position.X, Position.Y + n + 1),
                                Framing.GetTileSafely(Position.X + 1, Position.Y + n + 1)];///FINISH THIS
            Tile[] Tiles625p = [Framing.GetTileSafely(Position.X - 1, Position.Y + n),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + n),
                                Framing.GetTileSafely(Position.X - 1, Position.Y + n + 1),
                                Framing.GetTileSafely(Position.X + 2, Position.Y + n + 1),];
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
            seedSurroundingDifferenceDetailed = surroundingAreaData - seedData;
            var diff = seedSurroundingDifferenceDetailed.Clone();
            foreach (var seedTag in SeedTags.AllTags)
            {
                diff[seedTag] = Math.Abs(diff[seedTag]);
            }
            seedSurroundingDifference = diff.AverageLove;
        }

        private void CheckIfGrowenUp()
        {
            if (growthAmount >= 100)
            {

                int i = Position.X;
                int j = Position.Y;

                WorldGen.KillTile(i, j, false, false, true);
                // Place the new tile

                saplingPosToGrow.Add((i, j, seedData, seedSurroundingDifferenceDetailed));
            }
        }

        private static void GrowSapling(int i, int j, SeedData sd, SeedData errorSd)
        {
            Random r = new Random();
            int rInt = r.Next(0, 3);
            if (WorldGen.PlaceObject(i, j, ModContent.TileType<SaplingTile>(), mute: true, random: rInt))
            {
                // Get the ID of the newly placed tile entity
                int id = ModContent.GetInstance<SaplingEntity>().Place(i, j - 1);
                if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is SaplingEntity saplingEntity)
                {
                    RandomizeRedusingError(sd, errorSd, r, saplingEntity);
                }
                NetMessage.SendObjectPlacement(-1, i, j, ModContent.TileType<SaplingTile>(), 0, 0, -1, -1);
            }
        }

        private static void RandomizeRedusingError(SeedData sd, SeedData errorSd, Random r, SaplingEntity seedlingEntity)
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

    }

}
