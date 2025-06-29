using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Sunflowerology.Content.Structures
{
    public sealed class TwoSunflowerTrees : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int skyIslandIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));

            if (skyIslandIndex != -1)
                tasks.Insert(skyIslandIndex + 1, new DaylandTestHousePass("Sunflowerology: Two Sunflower Trees", loadWeight: 100f));
        }

        //public override void ClearWorld()
        //{
        //    // doesnt work probably, ClearWorld() might be the wrong hook even though it says to use it...
        //    StructureHelper.API.Generator.GenerateStructure(
        //            path: "Content/Structures/DaylandTestHouse",
        //            pos: new Point16(100, 100),
        //            mod: ModContent.GetInstance<Sunflowerology>()
        //    );
        //}

        private sealed class DaylandTestHousePass(string name, float loadWeight) : GenPass(name, loadWeight)
        {
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating sunflower trees...";
                PlaceTwoSunflowerTrees();
            }

            private static void PlaceTwoSunflowerTrees()
            {
                var mod = ModContent.GetInstance<Sunflowerology>();
                string structurePath = "Content/Structures/TwoSunTrees";

                // Get structure dimensions
                Point16 size = StructureHelper.API.Generator.GetStructureDimensions(path: structurePath, mod: mod);

                // Get spawn tile and offset 10 tiles to the right
                int startX = Main.spawnTileX + 10;
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
