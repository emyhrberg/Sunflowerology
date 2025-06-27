using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Sunflowerology.Content.Tiles.SunGrass;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.DeadSunflower
{
    internal class IceflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.IceBarrier;

        protected override int EffetDuration => 120;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Iceflower;
    }
}