using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using StructureHelper.Content.GUI;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ScienceJam.Content.Items.SunflowerSeeds
{
    internal class SunflowerSeed : SeedItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            seedData = NatureData.TypeOfSunflowerToData[TypeOfSunflower.Sunflower];
        }
    }
}
