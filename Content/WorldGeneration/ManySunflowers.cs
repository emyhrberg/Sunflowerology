using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Sunflowerology.Content.WorldGeneration
{
    public sealed class ManySunflowers : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int skyIslandIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));

            if (skyIslandIndex != -1)
                tasks.Insert(skyIslandIndex + 1, new DaylandTestHousePass("Sunflowerology: Many Trees", loadWeight: 100f));
        }

        private sealed class DaylandTestHousePass(string name, float loadWeight) : GenPass(name, loadWeight)
        {
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating more sunflower trees...";
                PlaceTwoSunflowerTrees();
            }

            private static void PlaceTwoSunflowerTrees()
            {
                var mod = ModContent.GetInstance<Sunflowerology>();
                string structurePath = "Content/Structures/ManySunflowers";

                // Get structure dimensions
                Point16 size = StructureHelper.API.Generator.GetStructureDimensions(path: structurePath, mod: mod);

                // Get spawn tile and offset 30 tiles to the right
                int startX = Main.spawnTileX + 30;
                int y = Main.spawnTileY;

                // Search downward for a grasstile
                while (y < Main.maxTilesY - 10)
                {
                    if (Main.tile[startX, y].HasTile && Main.tile[startX, y].TileType == TileID.Grass)
                    {
                        break;
                    }
                    y++;
                }

                // Align structure so it sits on top of the found sungrass
                int placeX = startX;
                int placeY = y - size.Y; // Align the bottom of the structure

                StructureHelper.API.Generator.GenerateStructure(
                    path: structurePath,
                    pos: new Point16(placeX, placeY),
                    mod: mod
                );
            }

        }
    }
}
