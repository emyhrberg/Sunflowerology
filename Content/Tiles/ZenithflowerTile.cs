using System.Collections.Generic;
using Sunflowerology.Content.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles
{
    internal class ZenithflowerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Asphalt;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 8;
            TileObjectData.newTile.FlattenAnchors = true;
            TileObjectData.newTile.Origin = new(0, 7);
            TileObjectData.newTile.AnchorBottom = new(Terraria.Enums.AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;
            //TileObjectData.newTile.StyleWrapLimit = 3;
            //TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                return;

            Player player = Main.LocalPlayer;

            float rangePixels = 20 * 16;
            Vector2 tileCenter = new((i + 1) * 16f, (j + 1) * 16f);

            bool withinRange = Vector2.Distance(player.Center, tileCenter) <= rangePixels;
            if (withinRange)
            {
                int dur = 30; // Duration of the buffs in frames (1 second = 60 frames)
                player.AddBuff(BuffID.Flipper, dur);
                player.AddBuff(BuffID.Calm, dur);
                player.AddBuff(BuffID.Inferno, dur);
                //player.AddBuff(BuffID.IceBarrier, dur);
                player.AddBuff(BuffID.ObsidianSkin, dur);
                player.AddBuff(BuffID.Gills, dur);
                player.AddBuff(BuffID.Warmth, dur);
                player.AddBuff(BuffID.TikiSpirit, dur);
                // player.AddBuff(BuffID.Happy, duration);
            }
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(ModContent.ItemType<ZenithflowerItem>());
        }
    }
}