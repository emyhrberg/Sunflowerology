using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ScienceJam.Content.WorldGeneration
{
    public sealed class GenerateGiantSunflowers : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int skyIslandIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Floating Island Houses"));

            if (skyIslandIndex != -1)
                tasks.Insert(skyIslandIndex + 1, new GiantSunflowersPass("Giant Sunflowers", 100f));
        }

        private sealed class GiantSunflowersPass(string name, float loadWeight) : GenPass(name, loadWeight)
        {
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating Giant Sunflowers";
                PlaceGiantSunflowers();
            }

            private static void PlaceGiantSunflowers()
            {
                // Place three 10 tiles left of spawn, next to each other
                for (int i = 0; i < 3; i++)
                {
                    int offsetX = Main.spawnTileX - 10 - (i * 2); // Adjust spacing between sunflowers
                    int offsetY = WorldGenHelper.FindSurface(offsetX);
                    WorldGenHelper.PlaceMultitile(
                        new Point16(offsetX, offsetY),
                        ModContent.TileType<Tiles.GiantSunflowerTile>(),
                        Main.rand.Next(3) // Random style (0-2)
                    );
                }

                // Place three 10 tiles right of spawn, next to each other
                for (int i = 0; i < 3; i++)
                {
                    int offsetX = Main.spawnTileX + 10 + (i * 2); // Adjust spacing between sunflowers
                    int offsetY = WorldGenHelper.FindSurface(offsetX);
                    WorldGenHelper.PlaceMultitile(
                        new Point16(offsetX, offsetY),
                        ModContent.TileType<Tiles.GiantSunflowerTile>(),
                        Main.rand.Next(3) // Random style (0-2)
                    );
                }
            }
        }
    }
}