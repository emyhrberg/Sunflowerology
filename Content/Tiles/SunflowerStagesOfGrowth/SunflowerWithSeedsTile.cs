using Sunflowerology.Content.Items.Sunflowers;
using Sunflowerology.Content.Items.SunflowerSeeds;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

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
                [TypeOfSunflower.Fireflower] = (BuffID.Inferno, 30),
                [TypeOfSunflower.Beachflower] = (BuffID.Flipper, 30),
                [TypeOfSunflower.Deadflower] = (ModContent.BuffType<Buffs.DeadSunflowerBuff>(), 30), // Reduced life
                [TypeOfSunflower.Dryflower] = (BuffID.Endurance, 30),
                [TypeOfSunflower.Iceflower] = (BuffID.Heartreach, 30),
                [TypeOfSunflower.Obsidianflower] = (BuffID.ObsidianSkin, 30),
                [TypeOfSunflower.Oceanflower] = (BuffID.Gills, 30),
                [TypeOfSunflower.Snowflower] = (BuffID.Mining, 30),
                [TypeOfSunflower.Sporeflower] = (BuffID.DryadsWard, 30),
                [TypeOfSunflower.Sunflower] = (BuffID.Sunflower, 30), // Happy!
            };

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            var tileData = TileObjectData.GetTileData(Type, 0);
            tileData.FlattenAnchors = true;
            tileData.Origin = new(0, 3);
            tileData.AnchorBottom = new(Terraria.Enums.AnchorType.SolidTile, tileData.Width, 0);
        }

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
                float rangePixels = 60f * 16f; // example range
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

            if (TileEntity.ByID.TryGetValue(id, out TileEntity te) && te is SunflowerWithSeedsEntity ste)
            {
                ste.typeOfSunflower = !ste.IsDead ? ste.plantData.FindClosestTypeOfSunflower() : TypeOfSunflower.Deadflower;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Item.NewItem(
                        new EntitySource_TileBreak(i, j),
                        i * 16, j * 16, 16, 16,
                        NatureData.TypeOfSunflowerToItemId[ste.typeOfSunflower],
                        1
                    );
                    if (ste.amountOfSeeds > 0)
                    {
                        int seedItemIndex = Item.NewItem(
                            new EntitySource_TileBreak(i, j),
                            i * 16, j * 16, 16, 16,
                            NatureData.TypeOfSunflowerToSeedItemId[ste.typeOfSunflower],
                            ste.amountOfSeeds,
                            noBroadcast: true
                        );
                        (Main.item[seedItemIndex].ModItem as SeedItem).seedData = ste.plantData.Clone();
                        NetMessage.SendData(
                            MessageID.SyncItem,
                            number: seedItemIndex
                        );
                    }
                }

            }
            else
            {
                Log.Warn($"TileEntity not found at {i}, {j} with ID: {id}, clientMode: {Main.netMode}");
            }
            base.KillMultiTile(i, j, frameX, frameY);
        }
        /*
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
        */

        public override void MouseOver(int i, int j)
        {
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            int id = ModContent.GetInstance<SunflowerWithSeedsEntity>().Find(i, j-3);

            if (TileEntity.ByID.TryGetValue(id, out TileEntity te) && te is SunflowerWithSeedsEntity ste)
            {
                if (item.ModItem is FlowerItem flowerItem &&
                    NatureData.TypeOfSunflowerToData.TryGetValue(flowerItem.TypeOfSunflower, out NatureData flowerData))
                {
                    ste.plantData = flowerData.Clone();
                }
                else
                {
                    ste.plantData = new NatureData();
                }
                ste.typeOfSunflower = ste.plantData.FindClosestTypeOfSunflower();
            }
        }

        public override bool CanDrop(int i, int j)
        {
            return false; // Prevents dropping the tile as an item
        }
    }
    public class SunflowerWithSeedsEntity : FinalPlantStageEntity
    {

        public int amountOfSeeds = 0;

        public override void LoadData(TagCompound tag)
        {
            base.LoadData(tag);
            if (tag.TryGet(nameof(amountOfSeeds), out int seeds))
            {
                amountOfSeeds = seeds;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            base.SaveData(tag);
            tag[nameof(amountOfSeeds)] = amountOfSeeds;
        }

        public override void NetSend(BinaryWriter writer)
        {
            base.NetSend(writer);
            writer.Write(amountOfSeeds);
        }

        public override void NetReceive(BinaryReader reader)
        {
            base.NetReceive(reader);
            amountOfSeeds = reader.ReadInt32();
        }

        protected override int TileType => ModContent.TileType<SunflowerWithSeedsTile>();
    }

}
