using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace Sunflowerology.Content.Tiles.DeadSunflower
{
    internal class ObsidianflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.ObsidianSkin;

        protected override int EffetDuration => 30;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Obsidianflower;
    }
}