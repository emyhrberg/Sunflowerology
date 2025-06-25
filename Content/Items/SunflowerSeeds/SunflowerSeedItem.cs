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
    internal class SunflowerSeedItem : SeedItem
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            seedData = new NatureData
            {
                [NatureTags.Dry] = 30,
                [NatureTags.Water] = 30,
                [NatureTags.Wild] = 20,
                [NatureTags.Sun] = 80,
                [NatureTags.Cave] = 10,
                [NatureTags.Hot] = 10,
                [NatureTags.Cold] = 10,
                [NatureTags.Evil] = 0,
                [NatureTags.Good] = 20,
                [NatureTags.Honey] = 0
            };
        }
    }
}
