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
                player.cursorItemIconText = $"{tileEntity.growthAmount1}, {tileEntity.growthAmount2}";
            }
        }
    }
    public class SeedlingEntity : ModTileEntity
    {
        public int growthAmount1 = 0;
        public int growthAmount2 = 0;
        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<SeedlingTile>();
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(growthAmount1)] = growthAmount1;
            tag[nameof(growthAmount2)] = growthAmount2;
        }

        public override void LoadData(TagCompound tag)
        {
            growthAmount1 = tag.GetInt(nameof(growthAmount1));
            growthAmount2 = tag.GetInt(nameof(growthAmount2));
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(growthAmount1);
            writer.Write(growthAmount2);

        }

        public override void NetReceive(BinaryReader reader)
        {
            growthAmount1 = reader.ReadInt32();
            growthAmount2 = reader.ReadInt32();
        }

    }

}
