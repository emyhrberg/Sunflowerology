using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Biomes
{
    public class DaylandDroplet : ModGore
    {
        public override void SetStaticDefaults()
        {
            ChildSafety.SafeGore[Type] = true;
            GoreID.Sets.LiquidDroplet[Type] = true;

            // Rather than copy in all the droplet specific gore logic, this gore will pretend to be another gore to inherit that logic.
            UpdateType = GoreID.WaterDrip;
        }
    }
}
