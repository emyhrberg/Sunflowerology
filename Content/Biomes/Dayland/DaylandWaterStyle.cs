using ScienceJam.Content.Dusts;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam.Content.Biomes
{
    public class DaylandWaterStyle : ModWaterStyle
    {


        private Asset<Texture2D> rainTexture;
        public override void Load()
        {
            rainTexture = Mod.Assets.Request<Texture2D>("Content/Biomes/DaylandRain");
        }

        public override int ChooseWaterfallStyle()
        {
            return ModContent.GetInstance<DaylandWaterfallStyle>().Slot;
        }

        public override int GetSplashDust()
        {
            return ModContent.DustType<DaylandSolution>();
        }

        public override int GetDropletGore()
        {
            return ModContent.GoreType<DaylandDroplet>();
        }

        public override void LightColorMultiplier(ref float r, ref float g, ref float b)
        {
            r = Main.DiscoColor.R / 255f;
            g = Main.DiscoColor.G / 255f;
            b = Main.DiscoColor.B / 255f;
        }

        public override Color BiomeHairColor()
        {
            return Color.White;
        }

        public override byte GetRainVariant()
        {
            return (byte)Main.rand.Next(3);
        }

        public override Asset<Texture2D> GetRainTexture() => rainTexture;
    }
}