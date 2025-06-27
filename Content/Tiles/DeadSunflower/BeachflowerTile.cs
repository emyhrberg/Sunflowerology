using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

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