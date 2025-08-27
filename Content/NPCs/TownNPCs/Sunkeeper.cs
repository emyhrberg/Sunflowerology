using Sunflowerology.Content.Items.HerbSeeds;
using Sunflowerology.Content.Items.SunflowerSeeds;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Sunflowerology.Content.NPCs.TownNPCs
{
    [AutoloadHead]
    public class Sunkeeper : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 25; // The total amount of frames the NPC has. You may need to change this based on how many frames your sprite sheet has.

            NPCID.Sets.ExtraFramesCount[Type] = 9; // These are the frames for raising their arm, sitting, talking, blinking, and attack. This is the remaining number of frames after the walking frames.
            NPCID.Sets.AttackFrameCount[Type] = 4; // The amount of frames in the attacking animation.
            NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the NPC that it tries to attack enemies. There are 16 pixels in 1 tile so a range of 700 is almost 44 tiles.
            NPCID.Sets.AttackType[Type] = 0; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
            NPCID.Sets.AttackTime[Type] = 90; // The amount of time it takes for the NPC's attack animation to be over once it starts. Measured in ticks. There are 60 ticks per second, so an amount of 90 will take 1.5 seconds.
            NPCID.Sets.AttackAverageChance[Type] = 30; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.
            NPCID.Sets.HatOffsetY[Type] = 4; // For when a party is active, the party hat spawns at a Y offset. Adjust this number to change where on your NPC's head the party hat sits.

            //NPCID.Sets.AllowDoorInteraction[Type] = false; // Disallow the NPC from opening doors
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true; // Sets NPC to be a Town NPC
            NPC.friendly = true; // The NPC will not attack player
            NPC.width = 18; // The width of the hitbox (hurtbox)
            NPC.height = 40; // The height of the hitbox (hurtbox)
            NPC.aiStyle = NPCAIStyleID.Passive; // Copies the AI of passive NPCs. This is AI Style 7.
            NPC.damage = 10; // This is the amount of damage the NPC will deal as contact damage. This is NOT the damage dealt by the Town NPC's attack.
            NPC.defense = 15; // All vanilla Town NPCs have a base defense of 15. This will increases as more bosses are defeated.
            NPC.lifeMax = 250; // All vanilla Town NPCs have 250 HP.
            NPC.HitSound = SoundID.NPCHit1; // The sound that is played when the NPC takes damage.
            NPC.DeathSound = SoundID.NPCDeath1; // The sound that is played with the NPC dies.
            NPC.knockBackResist = 0.5f; // All vanilla Town NPCs have 50% knockback resistance. Think of this more as knockback susceptibility. 1f = 100% knockback taken, 0f = 0% knockback taken.
            AnimationType = NPCID.Guide; // Sets the animation style to follow the animation of your chosen vanilla Town NPC.
        }

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            return true;
        }

        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = "Shop";
        }

        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            if (firstButton)
            {
                shopName = "Shop";
            }
        }

        public override void AddShops()
        {
            var npcShop = new NPCShop(Type, "Shop")
                .Add(new Item(ModContent.ItemType<SunflowerSeed>()) { shopCustomPrice = Item.buyPrice(silver: 10) })
                .Add(new Item(ModContent.ItemType<SunGrassSeed>()) { shopCustomPrice = Item.buyPrice(silver: 25) });

            npcShop.Register();
        }

        public override string GetChat()
        {
            WeightedRandom<string> chat = new();

            chat.Add("Hi there! Need some help with growing stuff?");
            chat.Add("I've heard there's some strange trees growing around here...", 1.5);
            chat.Add("I better get me a lady...");
            chat.Add("Have you seen flowers that look like me? I'm one of a kind!", 1.5);
            chat.Add("Psst...Growing flowers in their right biome may be beneficial...", 0.3);

            return chat.Get();
        }
    }
}