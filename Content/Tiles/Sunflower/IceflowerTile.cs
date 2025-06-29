using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace Sunflowerology.Content.Tiles.Sunflower
{
    internal class IceflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.IceBarrier;

        protected override int EffetDuration => 120;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Iceflower;
    }
}