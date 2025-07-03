using System;
using System.Collections.Generic;
using Sunflowerology.Content.Items.SunflowerSeeds;
using Sunflowerology.Content.Tiles.Sunflower;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SunflowerWithSeedsTile : PlantStageTile<SunflowerWithSeedsEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 4;
        protected override bool IsSpecialHook => false;
        protected override bool HaveGlow => true;
        protected override int[] Heights => [16, 16, 16, 18];

        // Buff mapping for each sunflower type
        private static readonly Dictionary<TypeOfSunflower, (int buffId, int buffDuration)> SunflowerBuffs
            = new()
            {
                [TypeOfSunflower.Fireflower] = (BuffID.OnFire, 30),
                [TypeOfSunflower.Beachflower] = (BuffID.Flipper, 30),
                [TypeOfSunflower.Deadflower] = (ModContent.BuffType<Buffs.DeadSunflowerBuff>(), 30), // Reduced life (-5 HP)
                [TypeOfSunflower.Dryflower] = (BuffID.Calm, 30),
                [TypeOfSunflower.Iceflower] = (BuffID.IceBarrier, 30),
                [TypeOfSunflower.Obsidianflower] = (BuffID.ObsidianSkin, 30),
                [TypeOfSunflower.Oceanflower] = (BuffID.Gills, 30),
                [TypeOfSunflower.Snowflower] = (BuffID.Warmth, 30),
                [TypeOfSunflower.Sporeflower] = (BuffID.TikiSpirit, 30),
                [TypeOfSunflower.Sunflower] = (BuffID.Sunflower, 30), // Happy!
            };

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                return;

            if (!TileEntity.TryGet(i, j, out TileEntity te) || te is not SunflowerWithSeedsEntity swe)
                return;

            // Get the sunflower type from the TileEntity
            TypeOfSunflower type = swe.typeOfSunflower;

            // For demonstration
            //Main.NewText($"Found type: {type}");

            // Apply buff based on type
            if (SunflowerBuffs.TryGetValue(type, out var buffData))
            {
                float rangePixels = 20f * 16f; // example range
                Player player = Main.LocalPlayer;
                Vector2 tileCenter = new((i + 1) * 16f, (j + 1) * 16f);
                bool withinRange = Vector2.Distance(player.Center, tileCenter) <= rangePixels;

                if (withinRange)
                    player.AddBuff(buffData.buffId, buffData.buffDuration);
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int id = ModContent.GetInstance<SunflowerWithSeedsEntity>().Find(i, j);
            Main.NewText(id);

            if (TileEntity.ByID.TryGetValue(id, out TileEntity te) && te is SunflowerWithSeedsEntity ste)
            {
                var r = new Random();
                ste.typeOfSunflower = ste.plantData.FindClosestTypeOfSunflower();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {

                    int seedItemIndex = Item.NewItem(
                        new EntitySource_TileBreak(i, j),
                        i * 16, j * 16, 16, 16,
                        NatureData.TypeOfSunflowerToSeedItemId[ste.typeOfSunflower],
                        r.Next(5, 11),
                        noBroadcast: true
                    );
                    int flowerItemIndex = Item.NewItem(
                        new EntitySource_TileBreak(i, j),
                        i * 16, j * 16, 16, 16,
                        NatureData.TypeOfSunflowerToItemId[ste.typeOfSunflower],
                        1
                    );
                    (Main.item[seedItemIndex].ModItem as SeedItem).seedData = ste.plantData.Clone();
                    NetMessage.SendData(
                        MessageID.SyncItem,
                        number: seedItemIndex
                    );

                }

            }
            else
            {
                Log.Warn($"TileEntity not found at {i}, {j} with ID: {id}, clientMode: {Main.netMode}");
            }
            base.KillMultiTile(i, j, frameX, frameY);
        }
        protected override string GetmouseOverPlantText(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out SunflowerWithSeedsEntity tileEntity))
            {
                string res = $"";
                foreach (var seedTag in NatureTags.AllTags)
                {
                    res += $"\n{seedTag}: {tileEntity.plantData[seedTag]}";
                }
                return res;
            }
            else
            {
                return "No tile entity found.";
            }
        }
    }
    public class SunflowerWithSeedsEntity : FinalPlantStageEntity
    {
        protected override int TileType => ModContent.TileType<SunflowerWithSeedsTile>();
    }

}
