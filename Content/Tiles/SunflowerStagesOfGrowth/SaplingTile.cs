using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SaplingTile : PlantStageTile<SaplingEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 3;
        protected override bool IsSpecialHook => false;
        protected override bool HaveGlow => true;
        protected override int[] Heights => [16, 16, 18];
    }
    public class SaplingEntity : PlantStageEntity<SunflowerWithSeedsEntity>
    {
        protected override int TileType => ModContent.TileType<SaplingTile>();
        protected override int NextTileType => ModContent.TileType<SunflowerWithSeedsTile>();

        private static Random r = new Random();

        protected override bool PlaceNextTile(int i, int j, NatureData newPlantData, int randomStyle, int style)
        {
            if (!WorldGen.PlaceObject(i, j+3, NextTileType, mute: true, style: style, random: randomStyle))
            {
                Main.NewText("Failed to place the next tile.");
                return false;
            }
            //var tod = TileObjectData.GetTileData(NextTileType, style);
            var point = TileObjectData.TopLeft(i, j);
            var newEntity = GetEntityOn(point.X, point.Y);

            newEntity.plantData = newPlantData;
            newEntity.amountOfSeeds = r.Next(5, 11); // Randomly set the amount of seeds between 1 and 3
            NetMessage.SendObjectPlacement(-1, i, j, NextTileType, style, 0, randomStyle, -1);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: newEntity.ID);
            return true;
        }
    }

}
