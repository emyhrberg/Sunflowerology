using System.Collections.Generic;
using StructureHelper.API;
using Terraria;
using Terraria.DataStructures;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace ScienceJam.Content.Structures
{
    public sealed class DaylandTestHouseSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int skyIslandIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Floating Island Houses"));

            if (skyIslandIndex != -1)
                tasks.Insert(skyIslandIndex + 1, new DaylandTestHousePass("ScienceJam: Dayland Test House", 100f));
        }

        public override void ClearWorld()
        {
            // doesnt work probably, ClearWorld() might be the wrong hook even though it says to use it...
            StructureHelper.API.Generator.GenerateStructure(
                    path: "Content/Structures/DaylandTestHouse",
                    pos: new Point16(100, 100),
                    mod: ModContent.GetInstance<ScienceJam>()
            );
        }

        private sealed class DaylandTestHousePass(string name, float loadWeight) : GenPass(name, loadWeight)
        {
            protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
            {
                progress.Message = "Generating Dayland Test House";
                PlaceDaylandTestHouse();
            }

            private static void PlaceDaylandTestHouse()
            {
                // works only if we gen a new world, not if we load an existing one
                StructureHelper.API.Generator.GenerateStructure(
                                    path: "Content/Structures/DaylandTestHouse",
                                    pos: new Point16(100, 100),
                                    mod: ModContent.GetInstance<ScienceJam>()
                            );
            }
        }
    }
}