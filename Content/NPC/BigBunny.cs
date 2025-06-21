using Microsoft.Xna.Framework;
using ScienceJam.Content.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ScienceJam.Content.NPC
{
    internal class BigBunny : ModNPC
    {
        private enum ActionState
        {
            Walking,
            PreparingToJump,
            Jumping,
            Standing,
            Attacking,
            AttackJump
        }

        private enum Frame
        {
            Standing,
            StartMove,
            PreparingForJump,
            Jump,
            FlyingUp,
            Hover,
            FlyingDown,
        }
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        public ref float AI_AngerStatus => ref NPC.ai[2];

        private int maxSpeed = 2;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
            NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.Shimmerfly;
        }

        public override void SetDefaults()
        {
            NPC.width = 36;
            NPC.height = 36;
            NPC.aiStyle = -1;
            NPC.damage = 0;
            NPC.defense = 2;
            NPC.lifeMax = 100;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 50f;

            Banner = Type;
            //BannerItem = ModContent.ItemType<ExampleCustomAISlimeNPCBanner>();
        }
        public override void AI()
        {
            NPC.AI_003_Fighters();
            return;
            switch (AI_State)
            {
                case (float)ActionState.Walking:
                    Walking();
                    break;
                case (float)ActionState.PreparingToJump:
                    PreparingToJump();
                    break;
                case (float)ActionState.Jumping:
                    Jumping();
                    break;
                case (float)ActionState.Standing:
                    Standing();
                    break;
                case (float)ActionState.Attacking:
                    Attacking();
                    break;
                case (float)ActionState.AttackJump:
                    AttackJump();
                    break;
            }

            /*
            if(NPC.life < NPC.lifeMax*0.7 && NPC.aiStyle == NPCAIStyleID.Passive)
            {
                NPC.damage = 10;
                NPC.defense = 5;
                NPC.aiStyle = NPCAIStyleID.Mimic;
            }*/
        }

        private void AttackJump()
        {
            throw new NotImplementedException();
        }

        private void Attacking()
        {
            throw new NotImplementedException();
        }

        private void Walking()
        {
            float targetSpeed = maxSpeed * NPC.direction;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetSpeed, 0.1f);

            NPC.velocity = Collision.TileCollision(
                NPC.position,
                NPC.velocity,
                NPC.width,
                NPC.height*2,
                fallThrough: true,
                fall2: true
            );


            AI_Timer++;
            if (AI_Timer > 240)
            {
                AI_State = (float)ActionState.Standing;
                AI_Timer = 0;
                NPC.netUpdate = true;
            }
        }

        private void Standing()
        {
            NPC.velocity.X = 0;
            AI_Timer++;
            if (AI_Timer > 4)
            {
                NPC.direction = Main.rand.NextBool() ? 1 : -1;
                AI_State = (float)ActionState.Walking;
                AI_Timer = 0;
                NPC.netUpdate = true;
            }
        }

        private void Jumping()
        {
            throw new NotImplementedException();
        }

        private void PreparingToJump()
        {
            throw new NotImplementedException();
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
