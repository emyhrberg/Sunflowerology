using ScienceJam.Common.Configs;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam.Common.Players
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