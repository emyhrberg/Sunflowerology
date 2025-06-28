using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Tiles.DeadSunflower
{
    internal class DeadflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 4;

        protected override int EffectBuffID => ModContent.BuffType<DeadSunflowerBuff>();

        protected override int EffetDuration => 120;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Deadflower;
    }
}