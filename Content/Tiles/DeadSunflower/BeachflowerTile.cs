using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Content.Buffs;
using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using ScienceJam.Content.Tiles.SunGrass;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.DeadSunflower
{
    internal class BeachflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.Flipper;

        protected override int EffetDuration => 30;
        
        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Beachflower;
    }
}