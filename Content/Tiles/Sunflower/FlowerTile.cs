﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.Sunflower
{
    internal abstract class FlowerTile : ModTile
    {
        protected abstract TypeOfSunflower TypeOfSunflower { get; }
        public abstract int RangeOfEffectInTiles { get; }
        public abstract int EffectBuffID { get; }
        public abstract int EffectDuration { get; }

        private Asset<Texture2D> glowTexture;
        public override string Texture => "Sunflowerology/Content/Tiles/SunflowerStagesOfGrowth/SunflowerWithSeedsTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Asphalt;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.FlattenAnchors = true;
            TileObjectData.newTile.Origin = new(0, 3);
            TileObjectData.newTile.AnchorBottom = new(Terraria.Enums.AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleWrapLimit = 3;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(10, 10, 0));
            glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (!closer)
                return;

            Player player = Main.LocalPlayer;

            float rangePixels = RangeOfEffectInTiles * 16;
            Vector2 tileCenter = new((i + 1) * 16f, (j + 1) * 16f);

            bool withinRange = Vector2.Distance(player.Center, tileCenter) <= rangePixels;
            if (withinRange)
                player.AddBuff(EffectBuffID, timeToAdd: EffectDuration);
        }

        public override IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield return new Item(NatureData.TypeOfSunflowerToItemId[TypeOfSunflower]);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            spriteBatch.Draw(
                glowTexture.Value,
                new Vector2(i * 16 - (int)Main.screenPosition.X, j * 16 - (int)Main.screenPosition.Y + 2) + zero,
                new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, tile.TileFrameY == 54 ? 18 : 16),
                Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            base.KillMultiTile(i, j, frameX, frameY);
        }


    }
}