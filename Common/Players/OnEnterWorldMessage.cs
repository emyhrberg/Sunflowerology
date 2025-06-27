using Sunflowerology.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Common.Players
{
    internal class OnEnterWorldMessage : ModPlayer
    {
        public override void OnEnterWorld()
        {
            base.OnEnterWorld();

            if (!Conf.C.ShowMessageWhenEnteringWorld) return;

            string msg = "";
            msg += "\n";

            Main.NewText(msg, Color.LightGray);
        }
    }
}