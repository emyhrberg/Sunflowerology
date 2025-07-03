using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace Sunflowerology.Content.Tiles.Sunflower
{
    internal class IceflowerTile : FlowerTile
    {
        public override int RangeOfEffectInTiles => 20;

        public override int EffectBuffID => BuffID.IceBarrier;

        public override int EffectDuration => 120;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Iceflower;
    }
}