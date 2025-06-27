using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StructureHelper.Content.GUI;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Sunflowerology.Content.Items.SunflowerSeeds
{
    internal class BeachflowerSeed : SeedItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            seedData = NatureData.TypeOfSunflowerToData[TypeOfSunflower.Beachflower];
        }
    }
}
