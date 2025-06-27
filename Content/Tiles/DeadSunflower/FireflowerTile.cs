using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace ScienceJam.Content.Tiles.DeadSunflower
{
    internal class FireflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.Inferno;

        protected override int EffetDuration => 30;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Fireflower;
    }
}