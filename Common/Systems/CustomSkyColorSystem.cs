using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Systems
{
    internal class CustomSkyColorSystem : ModSystem
    {
        public override void OnWorldLoad()
        {
            On_Main.SetBackColor += SetDaylandCollor;
        }
        public override void OnWorldUnload()
        {
            On_Main.SetBackColor -= SetDaylandCollor;
        }

        public static void SetDaylandCollor(On_Main.orig_SetBackColor orig, Main.InfoToSetBackColor info, out Color sunColor, out Color moonColor)
        {
            float influence = SunGrassTileCount.SunGrassTileInfluence;

            if (influence < 0.01f)
            {
                orig(info, out sunColor, out moonColor);
                return;
            }
            double realTime = Main.time;
            bool realDayTime = Main.dayTime;

            Main.time = Main.dayLength / 2;
            Main.dayTime = true;

            orig(info, out sunColor, out moonColor);
            Vector4 colorOfMidday = Main.ColorOfTheSkies.ToVector4();

            float rate = 0.4f;

            Main.time = realTime;
            Main.dayTime = realDayTime;

            orig(info, out sunColor, out moonColor);
            Vector4 colorReal = Main.ColorOfTheSkies.ToVector4();
            colorReal.X = Math.Min(colorOfMidday.X, colorReal.X + (colorOfMidday.X * rate) * influence);
            colorReal.Y = Math.Min(colorOfMidday.Y, colorReal.Y + (colorOfMidday.Y * rate) * influence);
            colorReal.Z = Math.Min(colorOfMidday.Z, colorReal.Z + (colorOfMidday.Z * rate) * influence);

            Main.ColorOfTheSkies = new Color(colorReal);
        }

    }
}
