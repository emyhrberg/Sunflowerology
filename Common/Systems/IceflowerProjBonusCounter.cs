using System;
using Microsoft.Xna.Framework;
using Sunflowerology.Common.Configs;
using Sunflowerology.Content.Buffs;
using Sunflowerology.Content.Tiles;
using Sunflowerology.Content.Tiles.SunGrass;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Systems
{
    
    public class IceflowerProjBonusCounter : ModSystem
    {
        public static int ShockwaveSpawnCooldown;

        public const int ShockwaveSpawnCooldownTicks = 120;

        public const int ShockwaveSpawnCooldownTicksBuffed = 90;
        public override void PreUpdateProjectiles()
        {
            if (ShockwaveSpawnCooldown > 0)
                ShockwaveSpawnCooldown--;
        }

        public static void ResetCounter()
        {
            if(Main.LocalPlayer.HasBuff<SnowflowerBuff>())
                ShockwaveSpawnCooldown = ShockwaveSpawnCooldownTicksBuffed;
            else
                ShockwaveSpawnCooldown = ShockwaveSpawnCooldownTicks;
        }
    }
}
