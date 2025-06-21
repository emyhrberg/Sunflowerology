using ScienceJam.Content.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ScienceJam.Content.NPC
{
    internal class BigBunny : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7; // make sure to set this for your modnpcs.

            NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Shimmerfly;
        }

        public override void SetDefaults()
        {
            NPC.width = 36; // The width of the npc's hitbox (in pixels)
            NPC.height = 36; // The height of the npc's hitbox (in pixels)
            NPC.aiStyle = -1; // This npc has a completely unique AI, so we set this to -1. The default aiStyle 0 will face the player, which might conflict with custom AI code.
            NPC.damage = 7; // The amount of damage that this npc deals
            NPC.defense = 2; // The amount of defense that this npc has
            NPC.lifeMax = 100; // The amount of health that this npc has
            NPC.HitSound = SoundID.NPCHit1; // The sound the NPC will make when being hit.
            NPC.DeathSound = SoundID.NPCDeath1; // The sound the NPC will make when it dies.
            NPC.value = 50f; // How many copper coins the NPC will drop when killed.

            Banner = Type;
            //BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome(ModContent.GetInstance<DaylandSurfaceBiome>()))
            {
                return SpawnCondition.OverworldDaySlime.Chance * 0.5f;
            }
            return 0f;
        }
    }
}
