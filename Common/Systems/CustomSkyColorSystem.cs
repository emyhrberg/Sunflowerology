using Terraria;
using Terraria.ModLoader;

namespace ScienceJam.Common.Systems
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
            orig(info, out sunColor, out moonColor);
            if (influence > 0.01f && !Main.dayTime)
            {
                double time = Main.time;

                bool dayTime = Main.dayTime;

                Main.time = Main.time / Main.nightLength * Main.dayLength;
                Main.dayTime = true;

                orig(info, out sunColor, out moonColor);
                Color biomeTimeColor = new Color(Main.ColorOfTheSkies.ToVector4());
                biomeTimeColor.B = (byte)((float)biomeTimeColor.B * 0.8f);
                Main.time = time;
                Main.dayTime = dayTime;

                orig(info, out sunColor, out moonColor);
                moonColor = new Color((sunColor.ToVector4() * (1f - influence) + (sunColor.ToVector4() + Main.DiscoColor.ToVector4()) * influence / 2f));
                sunColor = moonColor;
                Main.ColorOfTheSkies = new Color(biomeTimeColor.ToVector4() * influence + Main.ColorOfTheSkies.ToVector4() * (1f - influence));

            }
        }

    }
}
