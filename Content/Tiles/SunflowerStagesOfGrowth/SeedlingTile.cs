using System;
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
                player.cursorItemIconText = $"";
                foreach (var seedTag in SeedTags.AllTags)
                {
                    player.cursorItemIconText += $"\n{seedTag}: {tileEntity.sproutData[seedTag]}";
                }
            }
        }
    }
    public class SeedlingEntity : ModTileEntity
    {
        public SeedData sproutData = new();
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SeedlingTile>();
        }

        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                tag[seedTag] = sproutData[seedTag];
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
                        sproutData[seedTag] = val;
                    }
                    else
                    {
                        sproutData[seedTag] = 0;
                    }
                }
                catch
                {
                    sproutData[seedTag] = 0; // If the tag is not found or fails, set it to 0
                }

            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                writer.Write(sproutData[seedTag]);
            }

        }

        public override void NetReceive(BinaryReader reader)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                sproutData[seedTag] = reader.ReadInt32();
            }
        }

    }

}
