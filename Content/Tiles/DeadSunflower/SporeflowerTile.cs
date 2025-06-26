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
    internal class SporeflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.TikiSpirit;

        protected override int EffetDuration => 30;
        
        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Sporeflower;
    }
}