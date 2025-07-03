using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Tiles.Sunflower
{
    internal class DeadflowerTile : FlowerTile
    {
        public override int RangeOfEffectInTiles => 4;

        public override int EffectBuffID => ModContent.BuffType<DeadSunflowerBuff>();

        public override int EffectDuration => 120;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Deadflower;
    }
}