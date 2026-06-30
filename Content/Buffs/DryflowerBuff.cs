using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace Sunflowerology.Content.Buffs
{
    internal class DryflowerBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffImmune[BuffID.Suffocation] = true;
            player.GetJumpState<DryJump>().Enable();
        }
    }
}


class DryJump : ExtraJump
{
    public override Position GetDefaultPosition() => BeforeBottleJumps;
    public override float GetDurationMultiplier(Player player)
    {
        return 1.7f;
    }
    public override void UpdateHorizontalSpeeds(Player player)
    {
        player.runAcceleration *= 3f;
        player.maxRunSpeed *= 1.75f;
    }
    public override void OnStarted(Player player, ref bool playSound)
    {
        int offsetY = player.height;
        if (player.gravDir == -1f)
            offsetY = 0;

        offsetY -= 16;

        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustDirect(player.position + new Vector2(-34f, offsetY), 102, 32, DustID.Cloud, -player.velocity.X * 0.5f, player.velocity.Y * 0.5f, 100, Color.SandyBrown, 1.5f);
            dust.velocity = dust.velocity * 0.5f - player.velocity * new Vector2(0.1f, 0.3f);
        }

        SpawnCloudPoof(player, player.Top + new Vector2(-16f, offsetY));
        SpawnCloudPoof(player, player.position + new Vector2(-36f, offsetY));
        SpawnCloudPoof(player, player.TopRight + new Vector2(4f, offsetY));
    }
    private static void SpawnCloudPoof(Player player, Vector2 position)
    {
        Gore gore = Gore.NewGoreDirect(player.GetSource_FromThis(), position, -player.velocity, Main.rand.Next(11, 14));
        gore.velocity.X = gore.velocity.X * 0.1f - player.velocity.X * 0.1f;
        gore.velocity.Y = gore.velocity.Y * 0.1f - player.velocity.Y * 0.05f;
    }
}