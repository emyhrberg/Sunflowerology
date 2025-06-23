using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
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
    internal class SaplingTile : ModTile
    {
        private Asset<Texture2D> glowTexture;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Grass;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<SaplingEntity>().Generic_HookPostPlaceMyPlayer;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));

            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");

        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
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
            ModContent.GetInstance<SaplingEntity>().Kill(i, j);
        }

        public override void MouseOver(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out SaplingEntity tileEntity))
            {
                Player player = Main.LocalPlayer;
                player.noThrow = 2;
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = -1;
                player.cursorItemIconText = $"Growth: {tileEntity.growthAmount}, Diff: {tileEntity.seedSurroundingDifference}";
                foreach (var seedTag in SeedTags.AllTags)
                {
                    player.cursorItemIconText += $"\n{seedTag}: {tileEntity.seedData[seedTag]}, S:{tileEntity.surroundingAreaData[seedTag]}";
                }
            }
        }
    }
    public class SaplingEntity : ModTileEntity
    {
        public SeedData seedData = new SeedData();
        public SeedData surroundingAreaData = new SeedData();
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
        public static List<(int, int, SeedData)> sunflowerPosToGrow = new();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SaplingTile>();
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
            foreach (var (i, j, sd) in sunflowerPosToGrow)
            {
                GrowSunflower(i, j, sd);
            }
            sunflowerPosToGrow.Clear();
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
            CalculateSurroundings();
            CalculateDifference();

            if (Main.rand.NextBool(10) || true)
            {
                growthAmount += 10;
            }
            CheckIfGrowenUp();
        }

        private void CalculateSurroundings()
        {
            surroundingAreaData = new SeedData();
            int n = 3;
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
            var diff = surroundingAreaData - seedData;
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
                var seedData1 = seedData;

                int i = Position.X;
                int j = Position.Y;

                WorldGen.KillTile(i, j, false, false, true);
                // Place the new tile

                sunflowerPosToGrow.Add((i, j, seedData1));
            }
        }

        private static void GrowSunflower(int i, int j, SeedData sd)
        {
            Random r = new Random();
            int rInt = r.Next(0, 3);
            if (WorldGen.PlaceObject(i, j, ModContent.TileType<SunflowerWithSeedsTile>(), mute: true, random: rInt))
            {
                // Get the ID of the newly placed tile enti ty
                int id = ModContent.GetInstance<SunflowerWithSeedsEntity>().Place(i, j - 1);
                if (id != -1 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is SunflowerWithSeedsEntity saplingEntity)
                {
                    saplingEntity.seedData = sd;
                }
                NetMessage.SendObjectPlacement(-1, i, j, ModContent.TileType<SaplingTile>(), 0, 0, -1, -1);
            }
        }

    }

}
