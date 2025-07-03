using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Sunflowerology.Content.WorldGeneration
{
    public sealed class WorldGenTaskSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            WorldGenHelper.InsertWorldGenPass(
                mod: Mod,
                tasks,
                placeAfterPass: "Water Plants",
                taskName: "Two Sunflower Trees",
                message: "Planting two sunflower trees...",
                action: PlaceTwoSunflowerTrees
            );

            WorldGenHelper.InsertWorldGenPass(
                mod: Mod,
                tasks,
                placeAfterPass: "Water Plants",
                taskName: "Many Sunflower Trees",
                message: "Planting many sunflower trees...",
                action: PlaceManySunflowerTrees
            );
        }

        private static void PlaceManySunflowerTrees(GenerationProgress progress)
        {
            // Get file path and mod instance
            var mod = ModContent.GetInstance<Sunflowerology>();
            string structurePath = "Content/WorldGeneration/ManySunflowers";

            // Get the starting position for placing the structure 
            // Offset 30 tiles to the right of the spawn tile
            int startX = Main.spawnTileX + 30;
            int y = Main.spawnTileY;

            // Search downward for a grass tile, starting at the spawn tile
            while (y < Main.maxTilesY - 10)
            {
                if (Main.tile[startX, y].HasTile && Main.tile[startX, y].TileType == TileID.Grass)
                    break;

                y++;
            }

            // Get structure dimensions to align it properly
            // Align the structure so it sits on top of the found grass tile
            Point16 size = StructureHelper.API.Generator.GetStructureDimensions(structurePath, mod);
            int placeX = startX;
            int placeY = y - size.Y + 3; // Move the structure down by 3 tiles

            Log.Info($"Placing ManySunflowers at: ({placeX}, {placeY}) with size: {size.X}x{size.Y}");

            StructureHelper.API.Generator.GenerateStructure(
            path: structurePath,
            pos: new Point16(placeX, placeY),
            mod: mod
            );
        }

        private static void PlaceTwoSunflowerTrees(GenerationProgress progress)
        {
            // Get file path and mod instance
            var mod = ModContent.GetInstance<Sunflowerology>();
            string structurePath = "Content/WorldGeneration/TwoSunflowerTrees";

            // Get the starting position for placing the structure 
            // Offset 10 tiles to the right of the spawn tile
            int startX = Main.spawnTileX + 10;
            int y = Main.spawnTileY;

            // Search downward for a grass tile, starting at the spawn tile
            while (y < Main.maxTilesY - 10)
            {
                if (Main.tile[startX, y].HasTile && Main.tile[startX, y].TileType == TileID.Grass)
                    break;

                y++;
            }

            // Get structure dimensions to align it properly
            // Align the structure so it sits on top of the found grass tile
            Point16 size = StructureHelper.API.Generator.GetStructureDimensions(structurePath, mod);
            int placeX = startX;
            int placeY = y - size.Y;

            Log.Info($"Placing TwoSunflowerTrees at: ({placeX}, {placeY}) with size: {size.X}x{size.Y}");

            StructureHelper.API.Generator.GenerateStructure(
                path: structurePath,
                pos: new Point16(placeX, placeY),
                mod: mod
            );
        }
    }
}
